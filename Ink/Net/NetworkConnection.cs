using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Net;
using System.Threading.Channels;
using Ink.Text;
using Ink.Util;
using Ink.Util.Extensions;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using Rena.Native.Buffers;
using Rena.Native.Buffers.Extensions;
using ZlibSharp;
using Org.BouncyCastle.Crypto.Parameters;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.ObjectPool;
using Ink.Net.Encryption;

namespace Ink.Net;

public abstract class NetworkConnection<TContext> : IConnection, IAsyncDisposable
{
    private readonly struct ConnectionDataPacket(int length, ConnectionFlags Flags, byte[] data) : IDisposable
    {
        public readonly int Length = length;
        public readonly ConnectionFlags Flags = Flags;
        public readonly byte[] Data = data;

        public bool IsEmpty
            => Data == null;

        public Span<byte> Span
            => Data.AsSpan(0, Length);

        public Memory<byte> Memory
            => Data.AsMemory(0, Length);

        public void Dispose()
        {
            if(Data != null)
                PooledBytes.Return(Data);
        }
    }

    private sealed class DefaultPipeObjectPolicy : PooledObjectPolicy<Pipe>
    {
        public override Pipe Create()
            => new ();

        public override bool Return(Pipe obj)
        {
            obj.Reset();
            return true;
        }
    }

    private const int TimeoutTicks = 60 * 20;

    private static readonly TextPart Disconnected = TextPart.String("Disconnected");
    private static readonly TextPart ConnectionResetByPeer = TextPart.String("Connection reset by peer");
    private static readonly TextPart TimedOut = TextPart.String("Timed out");

    private static readonly UnboundedChannelOptions SharedSendQueueOptions = new() { SingleReader = true };

    private static readonly ArrayPool<byte> PooledBytes = ArrayPool<byte>.Create(1024 * 1024, 2048); // HACK: Make this configurable?
    private static readonly ObjectPool<PooledArrayBufferWriter<byte>> PooledBufferWriters = new DefaultObjectPool<PooledArrayBufferWriter<byte>>(new PooledBufferWriterObjectPolicy<ArrayPool<byte>>(PooledBytes));
    private static readonly ObjectPool<Pipe> PooledPipes = ObjectPool.Create<Pipe>(new DefaultPipeObjectPolicy());
    private static readonly ObjectPool<EncryptionPair> PooledEncryptionPairs = ObjectPool.Create<EncryptionPair>(); 

    private static readonly ZlibDecoder CachedDecoder = new ZlibDecoder();
    private static readonly ZlibEncoder CachedEncoder = new ZlibEncoder();

    private readonly ConnectionContext context;
    private readonly ConnectionPacketHandler<TContext> packetHandler;
    private readonly Channel<ConnectionDataPacket> sendQueue;

    private PooledArrayBufferWriter<byte>? compressionWriter;
    
    private EncryptionPair? encryption;
    private EncryptionPair? decryption;
    private PooledArrayBufferWriter<byte>? encryptionWriter;
    private Pipe? inputEncryptionPipe;

    private PacketStateHandler<TContext> currentHandler;
    private bool terminationAcknowledged;
    private int fragmentedCachedPacketLength;
    private int compressionThreshold;
    private ConnectionFlags flags;
    private long ticksWithoutResponse;

    protected readonly ILogger logger;

    public string Id
        => context.ConnectionId;

    public EndPoint? RemoteEndPoint
        => context.RemoteEndPoint;

    public CancellationToken DisconnectionToken
        => context.ConnectionClosed;

    public bool IsConnected
        => !DisconnectionToken.IsCancellationRequested;

    protected NetworkConnection(ConnectionContext context, ConnectionPacketHandler<TContext> packetHandler, ILoggerFactory loggerFactory)
    {
        this.context = context;
        this.packetHandler = packetHandler;
        this.currentHandler = packetHandler.TryGet(NetworkState.Handshake, out PacketStateHandler<TContext>? handler) ? handler : throw new UnreachableException("No handshake handler?");
        this.logger = loggerFactory.CreateLogger(typeof(NetworkConnection<TContext>));;
        this.sendQueue = Channel.CreateUnbounded<ConnectionDataPacket>(SharedSendQueueOptions);
    }

    public void Tick()
    {
        if(this.ticksWithoutResponse++ >= TimeoutTicks)
        {
            Disconnect();            
            return;
        }

        this.currentHandler.Tick(this, ProvideHandlerContext());
    }

    protected abstract TContext ProvideHandlerContext();

    protected void EnableCompression(int threshold)
    {
        this.currentHandler.CompressionEnabled(this, ProvideHandlerContext(), threshold);

        if (threshold < 0)
        {
            this.flags &= ~ConnectionFlags.Compressed;
            this.compressionThreshold = threshold;
            return;
        }

        this.flags |= ConnectionFlags.Compressed;
        this.compressionThreshold = threshold;

        this.compressionWriter = PooledBufferWriters.Get();
    }

    protected void EnableEncryption(byte[] secretKey)
    {
        this.flags |= ConnectionFlags.Encrypted;
        KeyParameter key = new KeyParameter(secretKey);
        ParametersWithIV keyIv = new ParametersWithIV(key, secretKey);

        this.encryption = PooledEncryptionPairs.Get();
        this.encryption.Init(true, key, keyIv);

        this.decryption = PooledEncryptionPairs.Get();
        this.decryption.Init(false, key, keyIv);

        this.encryptionWriter = PooledBufferWriters.Get();
        this.inputEncryptionPipe = PooledPipes.Get();
    }

    public void Send<TPacket>(in TPacket packet)
        where TPacket : struct, IPacket<TPacket>
    {
        if (!IsConnected)
            return;

        // HACK: This is very hacky but works!
        PooledArrayBufferWriter<byte> pooledWriter = PooledBufferWriters.Get();

        // Console.WriteLine($"Sending {typeof(TPacket).FullName}");
        pooledWriter.WriteVarInteger(this.currentHandler.StateInfo.DirectedRegistry(TPacket.PacketDirection).GetId(TPacket.PacketLocation));
        packet.Write(pooledWriter);
        // Console.WriteLine($"{Convert.ToHexString(pooledWriter.WrittenSpan)}");

        int writtenCount = pooledWriter.WrittenCount;
        PooledArray<byte> data = pooledWriter.Detach();
        _ = this.sendQueue.Writer.TryWrite(new ConnectionDataPacket(writtenCount, this.flags, data.Array));

        PooledBufferWriters.Return(pooledWriter);
    }


    public Task HandleConnectionAsync()
        => Task.WhenAll(ReadTransportAsync(), WriteTransportAsync());

    protected void SwitchState(NetworkState state)
    {
        if(!IsConnected)
            return;

        if(!this.packetHandler.TryGet(state, out PacketStateHandler<TContext>? newStateHandler))
        {
            throw new InvalidOperationException();
        }

        this.currentHandler = newStateHandler;
        this.currentHandler.Setup(this, ProvideHandlerContext());
    }

    private async Task ReadTransportAsync()
    {
        try
        {
            PipeReader transportReader = this.context.Transport.Input;

            while (true)
            {
                ReadResult transportReadResult = await transportReader.ReadAsync(DisconnectionToken);
                ReadOnlySequence<byte> transportReadBuffer = transportReadResult.Buffer;

                SequencePosition transportConsumed;

                if(this.flags.HasFlagFast(ConnectionFlags.Encrypted))
                {
                    PipeWriter inputEncryptionWriter = this.inputEncryptionPipe!.Writer;
                    foreach (ReadOnlyMemory<byte> value in transportReadBuffer)
                    {
                        Memory<byte> decrypted = inputEncryptionWriter.GetMemory(value.Length);
                        
                        this.decryption!.ProcessEntireBlock(value.Span, decrypted.Span);
                        inputEncryptionWriter.Advance(value.Length);
                    }

                    await inputEncryptionWriter.FlushAsync();

                    transportConsumed = transportReadBuffer.End;

                    PipeReader inputEncryptionReader = this.inputEncryptionPipe!.Reader;
                    while(inputEncryptionReader.TryRead(out ReadResult encryptedReadResult))
                    {
                        ReadOnlySequence<byte> encryptedReadBuffer = encryptedReadResult.Buffer; 

                        SequencePosition encryptedConsumed = ConsumePackets(encryptedReadBuffer);

                        inputEncryptionReader.AdvanceTo(encryptedConsumed, encryptedReadBuffer.End);
                    }
                }
                else
                {
                    transportConsumed = ConsumePackets(transportReadBuffer);
                }

                if (transportReadResult.IsCompleted)
                    break;

                transportReader.AdvanceTo(transportConsumed, transportReadBuffer.End);
            }
        }
        catch (OperationCanceledException)
        {
            Disconnect();
        }
        catch (ConnectionResetException)
        {
            Disconnect(ConnectionResetByPeer);
        }
        catch(Exception e)
        {
            Console.WriteLine($"Exception while reading connection {e}");
        }
        finally
        {
            this.inputEncryptionPipe?.Writer.Complete();
            this.inputEncryptionPipe?.Reader.Complete();
            Disconnect();
        }
    }

    // FIXME: Refactor this mess one day, i'll go to sleep
    private SequencePosition ConsumePackets(ReadOnlySequence<byte> buffer)
    {
        SequenceReader<byte> reader = new(buffer);
        Span<byte> varIntBytes = stackalloc byte[VarInteger<uint>.BytesNeeded];
        SequencePosition consumed = buffer.Start;

        while (true)
        {
            if (reader.End)
                break;

            int length = this.fragmentedCachedPacketLength;

            if(length <= 0)
            {
                _ = reader.TryCopyTo(varIntBytes[0..(int)long.Min(varIntBytes.Length, reader.Remaining)]);

                if (VarInteger<uint>.TryDecode(varIntBytes, out int lengthBytesRead, out Unsafe.As<int, uint>(ref length)) != OperationStatus.Done)
                    break;
                
                reader.Advance(lengthBytesRead);
                consumed = reader.Position;
            }

            if (length <= 0) // Nothing? :( Invalid Data
            {
                Disconnect(TextPart.String($"Got frame with length 0"));
                break;
            }

            if (reader.Remaining < length)
            {
                this.fragmentedCachedPacketLength = length;
                break;
            }

            this.fragmentedCachedPacketLength = -1;

            // FIXME: I don't like this code duplication
            if (!this.flags.HasFlagFast(ConnectionFlags.Compressed))
            {
                HandleUncompressedPacket(ref reader, length);
                reader.Advance(length);
            }
            else
            {
                int dataLength = default;

                _ = reader.TryCopyTo(varIntBytes[0..(int)long.Min(varIntBytes.Length, reader.Remaining)]);
                if (VarInteger<uint>.TryDecode(varIntBytes, out int dataByteLength, out Unsafe.As<int, uint>(ref dataLength)) != OperationStatus.Done)
                    break;

                reader.Advance(dataByteLength);
                int compressedLength = length - dataByteLength;

                if (dataLength == 0)
                {
                    HandleUncompressedPacket(ref reader, compressedLength);
                }
                else
                {
                    using PooledArray<byte> pooledCompressedPayload = PooledBytes.Rent<byte>(compressedLength);
                    Span<byte> compressedPayload = pooledCompressedPayload.AsSpan()[..compressedLength];
                    _ = reader.TryCopyTo(compressedPayload);

                    using PooledArray<byte> pooledFullPayload = PooledBytes.Rent<byte>(dataLength);
                    Span<byte> fullPayload = pooledFullPayload.AsSpan()[..dataLength];

                    if(!CachedDecoder.TryDecompress(compressedPayload, fullPayload, out ZlibResult? result)
                    || !result.HasValue)
                    {
                        Disconnect(TextPart.String("Badly compressed packet."));
                        break;
                    }

                    if(result!.Value.Status != ZlibStatus.Ok)
                    {
                        Disconnect(TextPart.String($"Badly compressed packet. ZlibStatus: {result!.Value.Status}"));
                        break;
                    }

                    // FIXME: Check bytes written, are they same?

                    HandlePacket(fullPayload);
                }

                reader.Advance(compressedLength);
            }

            consumed = reader.Position;
        }

        return consumed;
    }

    void HandleUncompressedPacket(ref SequenceReader<byte> reader, int length)
    {
        using PooledArray<byte> rawData = PooledBytes.Rent<byte>(length);
        Span<byte> fullPayload = rawData.AsSpan()[0..length];
        _ = reader.TryCopyTo(fullPayload);

        HandlePacket(fullPayload);
    }

    private void HandlePacket(ReadOnlySpan<byte> fullPayload)
    {
        if (VarInteger<uint>.TryDecode(fullPayload, out int idBytesLength, out uint id) != OperationStatus.Done)
            return;

        ReadOnlySpan<byte> packetPayload = fullPayload[idBytesLength..];
        _ = this.currentHandler.TryHandle((int)id, packetPayload, this, ProvideHandlerContext());
        this.ticksWithoutResponse = 0;
    }

    private async Task WriteTransportAsync()
    {
        ChannelReader<ConnectionDataPacket> sendQueueReader = this.sendQueue.Reader;
        PipeWriter transportWriter = this.context.Transport.Output;

        try
        {
            while(await sendQueueReader.WaitToReadAsync())
            {
                while(sendQueueReader.TryRead(out ConnectionDataPacket packet))
                {
                    using (packet)
                    {
                        ConnectionFlags packetFlags = packet.Flags;
                        IBufferWriter<byte> output = packetFlags.HasFlagFast(ConnectionFlags.Encrypted) ? this.encryptionWriter! : transportWriter;

                        if (packetFlags.HasFlagFast(ConnectionFlags.Compressed))
                        {
                            if (packet.Length < this.compressionThreshold)
                            {
                                output.WriteVarInteger(packet.Length + 1);
                                output.WriteRaw((byte)0);
                                output.Write(packet.Span);
                            }
                            else
                            {
                                Span<byte> compressionTarget = compressionWriter!.GetSpan(packet.Span.Length); 
                                if(!CachedEncoder.TryCompress(packet.Span, compressionTarget, out ZlibResult? result)
                                || !result.HasValue)
                                {
                                    // FIXME: Do something else...
                                    Disconnect(TextPart.String("Unknown error while compressing packet."));
                                    continue;
                                }

                                if(result.Value.Status != ZlibStatus.Ok)
                                {
                                    Disconnect(TextPart.String($"Error while compressing packet: {result.Value.Status}"));
                                    continue;
                                }

                                compressionWriter.Advance((int) result.Value.BytesWritten);
                                output.WriteVarInteger(VarInteger<uint>.GetByteCount((uint)packet.Span.Length) + compressionWriter.WrittenCount); 
                                output.WriteVarInteger(packet.Span.Length);
                                output.Write(compressionWriter.WrittenSpan);

                                compressionWriter.Reset();
                            }
                        }
                        else
                        {
                            output.WriteVarInteger(packet.Length);
                            output.Write(packet.Span);
                        }

                        if(packetFlags.HasFlagFast(ConnectionFlags.Encrypted))
                        {
                            Span<byte> data = transportWriter.GetSpan(this.encryptionWriter!.WrittenCount);
                            this.encryption!.ProcessEntireBlock(this.encryptionWriter.WrittenSpan, data);
                            transportWriter.Advance(this.encryptionWriter.WrittenCount);
                            this.encryptionWriter.Reset();
                        }
                    }
                }

                await transportWriter.FlushAsync(DisconnectionToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            await transportWriter.CompleteAsync();
            Disconnect();
        }
    }

    public void Disconnect()
        => Disconnect(Disconnected);

    public void Disconnect(TextPart reason)
    {
        if(terminationAcknowledged)
            return;

        this.terminationAcknowledged = true;

        if(IsConnected)
        {
            this.currentHandler.Disconnected(this, ProvideHandlerContext(), reason);
        }

        this.sendQueue.Writer.Complete();
        this.currentHandler.Terminated(this, ProvideHandlerContext(), reason);
    }

    public void Abort()
        => Abort(Disconnected);

    public void Abort(TextPart reason)
    {
        if(this.terminationAcknowledged)
            return;

        this.terminationAcknowledged = true;

        this.sendQueue.Writer.Complete();
        this.context.Abort();
        this.currentHandler.Terminated(this, ProvideHandlerContext(), reason);
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        this.compressionWriter?.Dispose();
        this.encryptionWriter?.Dispose();

        if(this.flags.HasFlagFast(ConnectionFlags.Compressed))
        {
            PooledBufferWriters.Return(this.compressionWriter!);
        }

        if(this.flags.HasFlagFast(ConnectionFlags.Encrypted))
        {
            PooledEncryptionPairs.Return(this.encryption!);
            PooledEncryptionPairs.Return(this.decryption!);
            PooledPipes.Return(this.inputEncryptionPipe!);
            PooledBufferWriters.Return(this.encryptionWriter!);
        }

        return context.DisposeAsync();
    }
}

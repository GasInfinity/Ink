using Ink.Net;
using Ink.Net.Packets.Login;
using Ink.Net.Packets.Common;
using Ink.Text;
using System.Security.Cryptography;
using Ink.Util;
using Ink.Auth;
using Rena.Native.Extensions;

namespace Ink.Server.Net.Handlers;

public sealed class LoginServerStateHandler : ServerPacketStateHandler
{
    private enum State
    {
        Hello,
        Key,
        WaitingAuth,
        WaitingAck
    }

    private sealed class Context
    {
        public State State = State.Hello;
        public byte[]? Token;
        public Task<GameProfile?>? AuthTask;
    }

    private sealed class HelloPacketHandler : PacketHandler<ServerNetworkConnection.ServerConnectionContext, ServerboundHello>
    {
        public override void Handle(in ServerboundHello packet, IConnection connection, ServerNetworkConnection.ServerConnectionContext ctx)
        {
            Context c = ctx.StateContext!.CastUnsafe<Context>();

            if(c.State != State.Hello)
            {
                connection.Disconnect(TextPart.String($"Invalid {nameof(ServerboundHello)} packet."));
                return;
            }

            c.Token = RandomNumberGenerator.GetBytes(ServerConstants.ConnectionTokenLength);
            ctx.Profile = new(packet.PlayerUuid, packet.Name);

            // if(false)
            // {
            //     c.State = State.WaitingAck;
            //     connection.Send(new ClientboundLoginFinished(ctx.Profile.Id, ctx.Profile.Name, ctx.Profile.Properties));
            //     return;
            // }

            c.State = State.Key;
            connection.Send(new ClientboundHello(string.Empty, ctx.ServerKeyring.PublicKey, c.Token, false));
        }
    }

    private sealed class KeyPacketHandler : PacketHandler<ServerNetworkConnection.ServerConnectionContext, ServerboundKey>
    {
        public override void Handle(in ServerboundKey packet, IConnection connection, ServerNetworkConnection.ServerConnectionContext ctx)
        {
            Context c = ctx.StateContext!.CastUnsafe<Context>();

            if(c.State != State.Key)
            {
                connection.Disconnect(TextPart.String($"Invalid {nameof(ServerboundKey)} packet."));
                return;
            }

            Span<byte> supposedVerifyToken = stackalloc byte[ServerConstants.ConnectionTokenLength];

            if(!ctx.ServerKeyring.Keypair.TryDecrypt(packet.VerifyToken, supposedVerifyToken, RSAEncryptionPadding.Pkcs1, out int tokenBytesWritten)
            || tokenBytesWritten != supposedVerifyToken.Length)
            {
                connection.Abort(TextPart.String($"Error while decrypting {nameof(supposedVerifyToken)}"));
                return;
            }

            if(!supposedVerifyToken.SequenceEqual(c.Token!))
            {
                connection.Abort(TextPart.String($"Verify token does not match!")); // TODO: Better messages?
                return;
            }

            c.Token = null;

            Span<byte> sharedSecret = stackalloc byte[ctx.ServerKeyring.Keypair.GetMaxOutputSize()];
            if(!ctx.ServerKeyring.Keypair.TryDecrypt(packet.SharedSecret, sharedSecret, RSAEncryptionPadding.Pkcs1, out int secretBytesWritten))
            {
                connection.Abort(TextPart.String($"Error decrypting shared secret!"));
                return;
            }
            
            // HACK: Maybe use TryDecrypt again?
            byte[] decryptedSharedSecret = ctx.ServerKeyring.Keypair.Decrypt(packet.SharedSecret, RSAEncryptionPadding.Pkcs1);

            ctx.EnableEncryption(decryptedSharedSecret);

            string serverHash = Sha1Digest.ComputeServer(string.Empty, decryptedSharedSecret, ctx.ServerKeyring.PublicKey);

            if(true)
            {
                c.State = State.WaitingAck;
                connection.Send(new ClientboundLoginFinished(ctx.Profile));
                return;
            }

            // c.AuthTask = GameProfile.HasJoined(ctx.Profile.Name, serverHash, null, ctx.Connection.DisconnectionToken);
            // c.State = State.WaitingAuth;
        }
    }

    private sealed class LoginAcknowledgedPacketHandler : PacketHandler<ServerNetworkConnection.ServerConnectionContext, ServerboundLoginAcknowledged>
    {
        public override void Handle(in ServerboundLoginAcknowledged packet, IConnection connection, ServerNetworkConnection.ServerConnectionContext ctx)
        {
            Context c = ctx.StateContext!.CastUnsafe<Context>();

            if(c.State != State.WaitingAck) 
            {
                connection.Disconnect(TextPart.String($"Invalid {nameof(ServerboundLoginAcknowledged)} packet."));
                return;
            }

            ctx.SwitchState(NetworkState.Configuration);
        }
    }

    public LoginServerStateHandler()
        : base(NetworkStates.LoginStateInfo)
    {
        Register(new HelloPacketHandler());
        Register(new KeyPacketHandler());
        Register(new LoginAcknowledgedPacketHandler());
        Freeze();
    }

    public override void Setup(IConnection connection, ServerNetworkConnection.ServerConnectionContext ctx)
    {
        base.Setup(connection, ctx);

        if(ctx.ProtocolVersion != ServerConstants.ServerProtocol)
            connection.Disconnect(TextPart.String($"Outdated Client/Server! I'm on {ServerConstants.ServerProtocol.ToStringFast()}, you're on {ctx.ProtocolVersion.ToStringFast()}"));

        ctx.StateContext = new Context();
    }

    public override void Tick(IConnection connection, ServerNetworkConnection.ServerConnectionContext ctx)
    {
        base.Tick(connection, ctx);
        
        Context c = ctx.StateContext!.CastUnsafe<Context>();

        switch(c.State)
        {
            case State.WaitingAuth:
                {
                    Task<GameProfile?> authTask = c.AuthTask!;

                    if(authTask.IsFaulted)
                    {
                        connection.Disconnect(TextPart.String($"Internal error while authenticating: {authTask.Exception.Message}"));
                        break;
                    }

                    if(authTask.IsCompletedSuccessfully)
                    {
                        GameProfile? possibleProfile = (authTask as Task<GameProfile?>)!.Result;

                        if(possibleProfile is not GameProfile profile)
                        {
                            connection.Disconnect(TextPart.String($"Couldn't get profile. Are you logged in at minecraft.net?"));
                            break;
                        }

                        if(ctx.Profile.Name != profile.Name
                        || ctx.Profile.Id != profile.Id)
                        {
                            connection.Disconnect(TextPart.String($"Profile mismatch between client and session server. {ctx.Profile.Name}[{ctx.Profile.Id}]/{profile.Name}[{profile.Id}] "));
                            break;
                        }

                        ctx.Profile = profile;
                        connection.Send(new ClientboundLoginFinished(ctx.Profile));

                        c.AuthTask = null;
                        c.State = State.WaitingAck;
                    }
                    break;
                }
        }
    }

    public override void CompressionEnabled(IConnection connection, ServerNetworkConnection.ServerConnectionContext ctx, int newThreshold)
    {
        base.CompressionEnabled(connection, ctx, newThreshold);

        connection.Send(new ClientboundLoginCompression(newThreshold));
    }

    public override void Disconnected(IConnection connection, ServerNetworkConnection.ServerConnectionContext ctx, TextPart reason)
    {
        base.Disconnected(connection, ctx, reason);

        connection.Send(new ClientboundLoginDisconnect(reason));
    }

    public override void Terminated(IConnection connection, ServerNetworkConnection.ServerConnectionContext ctx, TextPart reason)
    {
        base.Terminated(connection, ctx, reason);

        ctx.Logger.Disconnected(connection.Id, reason.ToPlainText(IContentDataProvider.Null));
    }
}

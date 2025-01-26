using Ink.Net;
using Ink.Net.Packets.Play;
using Ink.Net.Packets.Common;
using Ink.Registries;
using Ink.Text;
using Rena.Native.Extensions;
using Ink.Server.Entities;
using Ink.Entities;
using Microsoft.Extensions.ObjectPool;
using System.Collections.Concurrent;
using Rena.Mathematics;
using Ink.Util.Extensions;
using Ink.Math;
using Ink.Blocks.State;
using Ink.Server.Worlds;
using Ink.Blocks;
using Ink.Util;
using Ink.Items;
using Ink.Net.Structures;
using Ink.Worlds;
using Ink.Server.Entities.Components;
using Ink.Entities.Components;

namespace Ink.Server.Net.Handlers;

public sealed class PlayServerStateHandler : ServerKeptAlivePacketStateHandler
{
    private interface IMainThreadPacketExecutor
    {
        void Run(ServerNetworkConnection.ServerConnectionContext ctx);
        void Return();
    }

    private interface IMainThreadPacketExecutor<TPacketHandler, TPacket> : IMainThreadPacketExecutor 
        where TPacket : struct, IPacket<TPacket>
        where TPacketHandler : class, IMainThreadPacketExecutor<TPacketHandler, TPacket>, new()
    {
        TPacketHandler WithPacket(in TPacket packet);

        static abstract TPacketHandler Get(in TPacket packet);
    }

    private abstract class MainThreadPacketExecutor<TPacketHandler, TPacket> : IMainThreadPacketExecutor<TPacketHandler, TPacket>
        where TPacket : struct, IPacket<TPacket>
        where TPacketHandler : class, IMainThreadPacketExecutor<TPacketHandler, TPacket>, new()
    {
        private static readonly ObjectPool<TPacketHandler> HandlersPool = new DefaultObjectPool<TPacketHandler>(new DefaultPooledObjectPolicy<TPacketHandler>(), 8192); // HACK: Why 8192?
        private TPacket packet;

        public ref TPacket Packet
            => ref this.packet;

        public static TPacketHandler Get(in TPacket packet)
            => HandlersPool.Get().WithPacket(packet);

        public TPacketHandler WithPacket(in TPacket packet)
        {
            this.packet = packet;
            return this.CastUnsafe<TPacketHandler>();
        }

        public void Run(ServerNetworkConnection.ServerConnectionContext ctx)
            => Run(this.packet, ctx);

        protected abstract void Run(in TPacket packet, ServerNetworkConnection.ServerConnectionContext ctx);

        public void Return()
            => HandlersPool.Return(this.CastUnsafe<TPacketHandler>());
    }

    private sealed class Context
    {
        public readonly ConcurrentQueue<IMainThreadPacketExecutor> ScheduledHandlers = new();
        public int LastTeleportReplied = -1;
        public int LastBlockAcknowledge = -1;
    }

    private sealed class MainThreadExecutorPacketHandler<TPacketExecutor, TPacket> : PacketHandler<ServerNetworkConnection.ServerConnectionContext, TPacket>
        where TPacket : struct, IPacket<TPacket>
        where TPacketExecutor : class, IMainThreadPacketExecutor<TPacketExecutor, TPacket>, new()
    {
        public override void Handle(in TPacket packet, IConnection connection, ServerNetworkConnection.ServerConnectionContext ctx)
        {
            Context c = ctx.StateContext!.CastUnsafe<Context>();
            c.ScheduledHandlers.Enqueue(TPacketExecutor.Get(packet));
        }
    }

    private sealed class PlayCustomPayloadPacketHandler : ServerCustomPayloadPacketHandler
    {
        protected override void Handle(Identifier channel, byte[] data, IConnection connection, ServerNetworkConnection.ServerConnectionContext ctx)
        { }
    }

    private sealed class ClientInformationPacketHandler : PacketHandler<ServerNetworkConnection.ServerConnectionContext, ServerboundClientInformation>
    {
        public override void Handle(in ServerboundClientInformation packet, IConnection connection, ServerNetworkConnection.ServerConnectionContext ctx)
        {
            ctx.ClientInformation = packet;
        }
    }

    private sealed class ChatPacketHandler : PacketHandler<ServerNetworkConnection.ServerConnectionContext, ServerboundChat>
    {
        public override void Handle(in ServerboundChat packet, IConnection connection, ServerNetworkConnection.ServerConnectionContext ctx)
        {
            ctx.GameHandler.Broadcast(new ClientboundSystemChat(TextPart.String(packet.Message), false));
        }
    }

    private sealed class ChunkBatchReceivedPacketExecutor : MainThreadPacketExecutor<ChunkBatchReceivedPacketExecutor, ServerboundChunkBatchReceived>
    {
        protected override void Run(in ServerboundChunkBatchReceived packet, ServerNetworkConnection.ServerConnectionContext ctx)
        {
            ref EntityChunkSenderComponent chunkSender = ref ctx.Player.ChunkSender;

            chunkSender.MaxUnacknowledgedBatches = int.Max(chunkSender.MaxUnacknowledgedBatches, 10);

            if(chunkSender.UnacknowledgedBatches <= 0)
                return;

            chunkSender.UnacknowledgedBatches--;
            chunkSender.DesiredChunksPerTick = float.Max(packet.ChunksPerTick, 1);
        }
    }

    private sealed class SwingArmPacketExecutor : MainThreadPacketExecutor<SwingArmPacketExecutor, ServerboundSwing>
    {
        protected override void Run(in ServerboundSwing packet, ServerNetworkConnection.ServerConnectionContext ctx)
        {
            // ctx.Player!.SetPlayerFlag(packet.Hand == (int)Hand.Main ? ServerPlayerEntity.ServerPlayerFlags.ClientSwingedMain : ServerPlayerEntity.ServerPlayerFlags.ClientSwingedOff);
        }
    }

    private sealed class PlayerCommandPacketExecutor : MainThreadPacketExecutor<PlayerCommandPacketExecutor, ServerboundPlayerCommand>
    {
        protected override void Run(in ServerboundPlayerCommand packet, ServerNetworkConnection.ServerConnectionContext ctx)
        {
            // ServerPlayerEntity player = ctx.Player!;
            //
            // if (player.EntityId != packet.EntityId) // TODO! Disconnect?
            //     return;
            //
            // switch ((PlayerCommandAction)packet.ActionId)
            // {
            //     case PlayerCommandAction.StartSneaking:
            //         {
            //             player.IsSneaking = true;
            //             break;
            //         }
            //     case PlayerCommandAction.StopSneaking:
            //         {
            //             player.IsSneaking = false;
            //             break;
            //         }
            //     case PlayerCommandAction.StartSprinting:
            //         {
            //             player.IsSprinting = true;
            //             break;
            //         }
            //     case PlayerCommandAction.StopSprinting:
            //         {
            //             player.IsSprinting = false;
            //             break;
            //         }
            // }
        }
    }

    private sealed class PlayerActionPacketExecutor : MainThreadPacketExecutor<PlayerActionPacketExecutor, ServerboundPlayerAction>
    {
        protected override void Run(in ServerboundPlayerAction packet, ServerNetworkConnection.ServerConnectionContext ctx)
        {
            // Context c = ctx.StateContext!.CastUnsafe<Context>();
            //
            // ServerPlayerEntity player = ctx.Player!;
            // ServerWorld world = player.World;
            // c.LastBlockAcknowledge = packet.Sequence;
            //
            // switch ((PlayerActionStatus)packet.Status)
            // {
            //     case PlayerActionStatus.StartedDigging:
            //         {
            //             if (player.CurrentGameMode == GameMode.Creative)
            //             {
            //                 player.TryBreakBlock(packet.Location);
            //             }
            //
            //             break;
            //         }
            //     case PlayerActionStatus.FinishedDigging:
            //         {
            //             player.TryBreakBlock(packet.Location);
            //             break;
            //         }
            //     case PlayerActionStatus.DropItem:
            //         {
            //             ItemEntity entity = world.SpawnEntity<ItemEntity>(player.Position + new Vec3<double>(0, player.EyeHeight, 0));
            //             entity.Slot = new ItemStack(1, player[Ink.Containers.EquipmentSlot.MainHand].Id);
            //             entity.Velocity = player.HeadForwardVector * 0.4;
            //             break;
            //         }
            //     case PlayerActionStatus.SwapHandItems:
            //         {
            //             (player.Inventory.HeldStack, player.Inventory.OffHandStack) = (player.Inventory.OffHandStack, player.Inventory.HeldStack);
            //             break;
            //         }
            // }
        }
    }

    private sealed class MovePlayerPosPacketExecutor : MainThreadPacketExecutor<MovePlayerPosPacketExecutor, ServerboundMovePlayerPos>
    {
        protected override void Run(in ServerboundMovePlayerPos packet, ServerNetworkConnection.ServerConnectionContext ctx)
        {
            ref EntityTransformComponent transform = ref ctx.Player.Player.Living.Base.Transform;
            transform.Position = new(packet.X, packet.FeetY, packet.Z);
            // Context c = ctx.StateContext!.CastUnsafe<Context>();
            // ServerPlayerEntity player = ctx.Player!;
            //
            // if (player.LastTeleportSent != c.LastTeleportReplied)
            //     return;
            //
            // Vec3<double> position = player.Position;
            //
            // double newX = packet.X;
            // double newY = packet.FeetY;
            // double newZ = packet.Z;
            //
            // double deltaX = newX - position.X;
            // double deltaY = newY - position.Y;
            // double deltaZ = newZ - position.Z;
            //
            // if (deltaX.AlmostEqual(0) && deltaY.AlmostEqual(0) && deltaZ.AlmostEqual(0))
            //     return; // Same position?
            //
            // if(double.Abs(deltaX) > 8 || double.Abs(deltaZ) > 8 || double.Abs(deltaY) > 10)
            // {
            //     player.SetPlayerFlag(ServerPlayerFlags.SyncronizePosition);
            //     return;
            // }
            //
            // player.Move(new(deltaX, deltaY, deltaZ));
            // Vec3<double> newServerPosition = player.Position;
            //
            // double newDeltaX = newX - newServerPosition.X;
            // double newDeltaY = newY - newServerPosition.Y;
            // double newDeltaZ = newZ - newServerPosition.Z;
            //
            // if(!newDeltaX.AlmostEqual(0) || !newDeltaY.AlmostEqual(0) || !newDeltaZ.AlmostEqual(0))
            // {
            //     player.SetPlayerFlag(ServerPlayerFlags.SyncronizePosition);
            //     return;
            // }
            //
            // player.Position = new(newX, newY, newZ);
            // // TODO: Onground?
        }
    }

    private sealed class MovePlayerRotPacketExecutor : MainThreadPacketExecutor<MovePlayerRotPacketExecutor, ServerboundMovePlayerRot>
    {
        protected override void Run(in ServerboundMovePlayerRot packet, ServerNetworkConnection.ServerConnectionContext ctx)
        {
            ref EntityTransformComponent transform = ref ctx.Player.Player.Living.Base.Transform;
            transform.Rotation = new(packet.Yaw, packet.Pitch);
            transform.HeadYaw = packet.Yaw;
            // Context c = ctx.StateContext!.CastUnsafe<Context>();
            // ServerPlayerEntity player = ctx.Player!;
            //
            // if (player.LastTeleportSent != c.LastTeleportReplied)
            //     return;
            //
            // Vec2<float> rotation = player.Rotation;
            //
            // float newYaw = packet.Yaw;
            // float newPitch = packet.Pitch;
            //
            // float deltaYaw = newYaw - rotation.X;
            // float deltaPitch = newPitch - rotation.Y;
            //
            // if (deltaYaw.AlmostEqual(0) && deltaPitch.AlmostEqual(0))
            //     return;
            //
            // player.Rotation = new(newYaw, newPitch);
            // player.CurrentHeadYaw = newYaw;
            // // TODO: Onground?
        }
    }

    private sealed class MovePlayerPosRotPacketExecutor : MainThreadPacketExecutor<MovePlayerPosRotPacketExecutor, ServerboundMovePlayerPosRot>
    {
        protected override void Run(in ServerboundMovePlayerPosRot packet, ServerNetworkConnection.ServerConnectionContext ctx)
        {
            ref EntityTransformComponent transform = ref ctx.Player.Player.Living.Base.Transform;
            transform.Position = new(packet.X, packet.FeetY, packet.Z);
            transform.Rotation = new(packet.Yaw, packet.Pitch);
            transform.HeadYaw = packet.Yaw;
            // Context c = ctx.StateContext!.CastUnsafe<Context>();
            // ServerPlayerEntity player = ctx.Player!;
            //
            // if (player.LastTeleportSent != c.LastTeleportReplied)
            //     return;
            // 
            // Vec3<double> position = player.Position;
            // Vec2<float> rotation = player.Rotation;
            //
            // double newX = packet.X;
            // double newY = packet.FeetY;
            // double newZ = packet.Z;
            // float newYaw = packet.Yaw;
            // float newPitch = packet.Pitch;
            //
            // float deltaYaw = newYaw - rotation.X;
            // float deltaPitch = newPitch - rotation.Y;
            // double deltaX = newX - position.X;
            // double deltaY = newY - position.Y;
            // double deltaZ = newZ - position.Z;
            //
            // if (deltaX.AlmostEqual(0) && deltaY.AlmostEqual(0) && deltaZ.AlmostEqual(0) && deltaYaw.AlmostEqual(0) && deltaPitch.AlmostEqual(0))
            //     return;
            // 
            // if (double.Abs(deltaX) > 8 || double.Abs(deltaZ) > 8 || double.Abs(deltaY) > 10)
            // {
            //     player.SetPlayerFlag(ServerPlayerFlags.SyncronizePosition);
            //     return;
            // }
            //
            // player.Move(new(deltaX, deltaY, deltaZ));
            // Vec3<double> newServerPosition = player.Position;
            //
            // double newDeltaX = newX - newServerPosition.X;
            // double newDeltaY = newY - newServerPosition.Y;
            // double newDeltaZ = newZ - newServerPosition.Z;
            //
            // if(!newDeltaX.AlmostEqual(0) || !newDeltaY.AlmostEqual(0) || !newDeltaZ.AlmostEqual(0))
            // {
            //     player.SetPlayerFlag(ServerPlayerFlags.SyncronizePosition);
            //     return;
            // }
            //
            // player.Position = new(newX, newY, newZ);
            // player.Rotation = new(newYaw, newPitch);
            // player.CurrentHeadYaw = newYaw;
            //
            // // TODO: Onground?
        }
    }

    // FIXME: Do something with the onground update packet

    private sealed class AcceptTeleportationPacketExecutor : MainThreadPacketExecutor<AcceptTeleportationPacketExecutor, ServerboundAcceptTeleportation>
    {
        protected override void Run(in ServerboundAcceptTeleportation packet, ServerNetworkConnection.ServerConnectionContext ctx)
        {
            Context c = ctx.StateContext!.CastUnsafe<Context>();
            c.LastTeleportReplied = packet.TeleportId;
        }
    }

    private sealed class SetCarriedItemPacketExecutor : MainThreadPacketExecutor<SetCarriedItemPacketExecutor, ServerboundSetCarriedItem>
    {
        protected override void Run(in ServerboundSetCarriedItem packet, ServerNetworkConnection.ServerConnectionContext ctx)
        {
            // ctx.Player!.AcknowledgeHeldSlot(packet.Slot);
        }
    }

    private sealed class UseItemOnPacketExecutor : MainThreadPacketExecutor<UseItemOnPacketExecutor, ServerboundUseItemOn>
    {
        protected override void Run(in ServerboundUseItemOn packet, ServerNetworkConnection.ServerConnectionContext ctx)
        {
            // Context c = ctx.StateContext!.CastUnsafe<Context>();
            // ServerPlayerEntity player = ctx.Player!;
            // ServerWorld world = player.World;
            // c.LastBlockAcknowledge = packet.Sequence;
            //
            // BlockPosition position = packet.Location;
            // Hand hand = (Hand)packet.Hand;
            //
            // if (!player.IsSneaking)
            // {
            //     BlockState state = world.GetBlockState(position);
            //     Block? block = state.GetBlock(world.BlockRegistry);
            //
            //     ActionResult blockUseResult = block?.OnUse(state, world, position, player, hand) ?? ActionResult.Pass;
            //
            //     if (blockUseResult.SwingsHand())
            //         player.Swing(Hand.Main);
            //
            //     if (blockUseResult != ActionResult.Pass)
            //         return;
            // }
            //
            // ItemStack main = player.Inventory.HeldStack;
            //
            // if (!main.IsEmpty)
            // {
            //     Item? mainItem = main.Item(world.RegistryManager.Item);
            //
            //     ActionResult<ItemStack> mainItemResult = mainItem?.UseOnBlock(main, player, world, packet.Location, (BlockFace)packet.Face, packet.CursorPositionX, packet.CursorPositionY, packet.CursorPositionZ, packet.InsideBlock) ?? ActionResult<ItemStack>.Pass;
            //
            //     if (mainItemResult.Result.SwingsHand())
            //         player.Swing(Hand.Main);
            //
            //     if (mainItemResult.Result.PerformsAction())
            //     {
            //         player.Inventory.HeldStack = mainItemResult.Value;
            //         return;
            //     }
            //
            //     if (mainItemResult.Result != ActionResult.Pass)
            //         return;
            // }
            //
            // ItemStack off = player.Inventory.OffHandStack;
            //
            // if (!off.IsEmpty)
            // {
            //     Item? offItem = off.Item(world.RegistryManager.Item);
            //     ActionResult<ItemStack> offItemResult = offItem?.UseOnBlock(main, player, world, packet.Location, (BlockFace)packet.Face, packet.CursorPositionX, packet.CursorPositionY, packet.CursorPositionZ, packet.InsideBlock) ?? ActionResult<ItemStack>.Pass;
            //
            //     if (offItemResult.Result.SwingsHand())
            //         player.Swing(Hand.Main);
            //
            //     if (offItemResult.Result.PerformsAction())
            //     {
            //         player.Inventory.OffHandStack = offItemResult.Value;
            //         return;
            //     }
            //
            //     if (offItemResult.Result != ActionResult.Pass)
            //         return;
            // }
        }
    }

    private sealed class UseItemPacketExecutor : MainThreadPacketExecutor<UseItemPacketExecutor, ServerboundUseItem>
    {
        protected override void Run(in ServerboundUseItem packet, ServerNetworkConnection.ServerConnectionContext ctx)
        {
            // Context c = ctx.StateContext!.CastUnsafe<Context>();
            // ServerPlayerEntity player = ctx.Player!;
            // ServerWorld world = player.World;
            // c.LastBlockAcknowledge = packet.Sequence;
            //
            // ItemStack main = player.Inventory.HeldStack;
            //
            // if (!main.IsEmpty)
            // {
            //     Item? mainitem = main.Item(world.RegistryManager.Item);
            //     ActionResult<ItemStack> mainItemResult = mainitem?.Use(main, world, player) ?? ActionResult<ItemStack>.Pass;
            //
            //     if (mainItemResult.Result.SwingsHand())
            //         player.Swing(Hand.Main);
            //
            //     if (mainItemResult.Result.PerformsAction())
            //     {
            //         player.Inventory.HeldStack = mainItemResult.Value;
            //         return;
            //     }
            //
            //     if (mainItemResult.Result != ActionResult.Pass)
            //         return;
            // }
            //
            // ItemStack off = player.Inventory.OffHandStack;
            //
            // if (!off.IsEmpty)
            // {
            //     Item? offItem = off.Item(world.RegistryManager.Item);
            //     ActionResult<ItemStack> offItemResult = offItem?.Use(main, world, player) ?? ActionResult<ItemStack>.Pass;
            //
            //     if (offItemResult.Result.SwingsHand())
            //         player.Swing(Hand.Main);
            //
            //     if (offItemResult.Result.PerformsAction())
            //     {
            //         player.Inventory.OffHandStack = offItemResult.Value;
            //         return;
            //     }
            //
            //     if (offItemResult.Result != ActionResult.Pass)
            //         return;
            // }
        }
    }

    private sealed class SetCreativeModeSlotExecutor : MainThreadPacketExecutor<SetCreativeModeSlotExecutor, ServerboundSetCreativeModeSlot>
    {
        protected override void Run(in ServerboundSetCreativeModeSlot packet, ServerNetworkConnection.ServerConnectionContext ctx)
        {
            // ServerPlayerEntity player = ctx.Player!;
            //
            // if(!player.PlayerViewHandler.TryHandleCreativeSetSlot(packet))
            // {
            //     // Event / Kick, Invalid!
            // }
        }
    }

    public PlayServerStateHandler()
        : base(NetworkStates.PlayStateInfo)
    {
        // Everything will only be instantiated once :D 
        Register(new PlayCustomPayloadPacketHandler());
        Register(new ClientInformationPacketHandler());
        Register(new ChatPacketHandler());
        Register(new MainThreadExecutorPacketHandler<ChunkBatchReceivedPacketExecutor, ServerboundChunkBatchReceived>());
        Register(new MainThreadExecutorPacketHandler<SwingArmPacketExecutor, ServerboundSwing>());
        Register(new MainThreadExecutorPacketHandler<PlayerActionPacketExecutor, ServerboundPlayerAction>());
        Register(new MainThreadExecutorPacketHandler<PlayerCommandPacketExecutor, ServerboundPlayerCommand>());
        Register(new MainThreadExecutorPacketHandler<MovePlayerPosPacketExecutor, ServerboundMovePlayerPos>());
        Register(new MainThreadExecutorPacketHandler<MovePlayerRotPacketExecutor, ServerboundMovePlayerRot>());
        Register(new MainThreadExecutorPacketHandler<MovePlayerPosRotPacketExecutor, ServerboundMovePlayerPosRot>());
        Register(new MainThreadExecutorPacketHandler<AcceptTeleportationPacketExecutor, ServerboundAcceptTeleportation>());
        Register(new MainThreadExecutorPacketHandler<SetCarriedItemPacketExecutor, ServerboundSetCarriedItem>());
        Register(new MainThreadExecutorPacketHandler<UseItemOnPacketExecutor, ServerboundUseItemOn>());
        Register(new MainThreadExecutorPacketHandler<UseItemPacketExecutor, ServerboundUseItem>());
        Register(new MainThreadExecutorPacketHandler<SetCreativeModeSlotExecutor, ServerboundSetCreativeModeSlot>());
        Freeze();
    }

    public override void Setup(IConnection connection, ServerNetworkConnection.ServerConnectionContext ctx)
    {
        base.Setup(connection, ctx);

        ctx.StateContext = new Context();
        ctx.GameHandler.QueuePlaying(ctx.Connection);
    }

    public override void Tick(IConnection connection, ServerNetworkConnection.ServerConnectionContext ctx)
    {
        base.Tick(connection, ctx);
        Context c = ctx.StateContext!.CastUnsafe<Context>();

        var scheduledHandlers = c.ScheduledHandlers;
        while(scheduledHandlers.TryDequeue(out IMainThreadPacketExecutor? executor))
        {
            executor.Run(ctx);
            executor.Return();
        }

        if(c.LastBlockAcknowledge != -1)
        {
            connection.Send(new ClientboundBlockChangedAck(c.LastBlockAcknowledge));
            c.LastBlockAcknowledge = -1;
        }
    }

    public override void Disconnected(IConnection connection, ServerNetworkConnection.ServerConnectionContext ctx, TextPart reason)
    {
        base.Disconnected(connection, ctx, reason);

        connection.Send(new ClientboundDisconnect(reason));
    }

    public override void Terminated(IConnection connection, ServerNetworkConnection.ServerConnectionContext ctx, TextPart reason)
    {
        base.Terminated(connection, ctx, reason);

        ctx.GameHandler.RemovePlaying(ctx.Connection);
    }

    // private void HandlePlay(ServerboundPlayPacketId id, ReadOnlySpan<byte> payload)
    // {
    //     switch(id)
    //     {
    //         case ServerboundPlayPacketId.ChatMessage:
    //             {
    //                 if (!SChatMessagePacket.TryRead(payload, out SChatMessagePacket chatMessage)) // Disconnect?
    //                     break;
    //
    //                 NetworkHandler.BroadcastPlay(new CSystemChatMessagePacket(TextPart.String($"<{this.player!.Username}>: {chatMessage.Message}"), false));
    //                 break;
    //             }
    //         case ServerboundPlayPacketId.ChatCommand:
    //             {
    //                 if (!SChatCommandPacket.TryRead(payload, out SChatCommandPacket _)) // Disconnect?
    //                     break;
    //
    //                 //string command = result.Command;
    //                 Disconnect(TextPart.String("Do not type commands! :)"));
    //                 break;
    //             }
    //         default:
    //             {
    //                 byte[] packetData = ArrayPool<byte>.Shared.Rent(payload.Length);
    //                 payload.CopyTo(packetData);
    //                 this.player!.QueueIncomingPlayPacket(new((int)id, payload.Length, packetData));
    //                 break;
    //             }
    //     }
    // }
}

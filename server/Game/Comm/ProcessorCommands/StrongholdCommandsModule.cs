#region

using System;
using System.Linq;
using Game.Data;
using Game.Data.Stronghold;
using Game.Data.Tribe;
using Game.Map;
using Game.Setup;
using Game.Util.Locking;

#endregion

namespace Game.Comm.ProcessorCommands
{
    class StrongholdCommandsModule : CommandModule
    {
        private readonly ILocker locker;

        private readonly IStrongholdManager strongholdManager;

        private readonly IWorld world;

        private readonly IThemeManager themeManager;

        public StrongholdCommandsModule(IStrongholdManager strongholdManager, IWorld world, ILocker locker, IThemeManager themeManager)
        {
            this.strongholdManager = strongholdManager;
            this.world = world;
            this.locker = locker;
            this.themeManager = themeManager;
        }

        public override void RegisterCommands(IProcessor processor)
        {
            processor.RegisterCommand(Command.StrongholdNameGet, GetName);
            processor.RegisterCommand(Command.StrongholdInfo, GetInfo);
            processor.RegisterCommand(Command.StrongholdInfoByName, GetInfoByName);
            processor.RegisterCommand(Command.StrongholdLocate, Locate);
            processor.RegisterCommand(Command.StrongholdGateRepair, GateRepair);
            processor.RegisterCommand(Command.StrongholdList, ListAll);
            processor.RegisterCommand(Command.StrongholdSetTheme, SetTheme);
        }

        private void ListAll(Session session, Packet packet)
        {
            locker.Lock(session.Player).Do(() =>
            {
                if (!session.Player.IsInTribe)
                {
                    ReplyError(session, packet, Error.TribesmanNotPartOfTribe);
                    return;
                }

                var reply = new Packet(packet);
                var strongholds = strongholdManager.StrongholdsForTribe(session.Player.Tribesman.Tribe).ToList();
                reply.AddInt16((short)strongholds.Count);
                foreach (var stronghold in strongholds)
                {
                    reply.AddUInt32(stronghold.ObjectId);
                    reply.AddString(stronghold.Name);
                    reply.AddByte(stronghold.Lvl);
                    reply.AddUInt32(stronghold.PrimaryPosition.X);
                    reply.AddUInt32(stronghold.PrimaryPosition.Y);
                }

                session.Write(reply);
            });
        }

        private void Locate(Session session, Packet packet)
        {
            var reply = new Packet(packet);

            uint strongholdId;
            string strongholdName = string.Empty;

            try
            {
                strongholdId = packet.GetUInt32();
                if (strongholdId == 0)
                {
                    strongholdName = packet.GetString();
                }
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            if (strongholdId == 0)
            {
                if (!world.FindStrongholdId(strongholdName, out strongholdId))
                {
                    ReplyError(session, packet, Error.StrongholdNotFound);
                    return;
                }
            }

            IStronghold stronghold;
            locker.Lock(strongholdId, out stronghold).Do(() =>
            {
                if (stronghold == null || stronghold.StrongholdState == StrongholdState.Inactive)
                {
                    ReplyError(session, packet, Error.StrongholdNotFound);
                    return;
                }

                reply.AddUInt32(stronghold.PrimaryPosition.X);
                reply.AddUInt32(stronghold.PrimaryPosition.Y);

                session.Write(reply);
            });
        }

        private void GetInfo(Session session, Packet packet)
        {
            var reply = new Packet(packet);
            uint strongholdId;

            try
            {
                strongholdId = packet.GetUInt32();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            if (session.Player.Tribesman == null)
            {
                ReplyError(session, packet, Error.TribeIsNull);
                return;
            }

            IStronghold stronghold;
            if (!strongholdManager.TryGetStronghold(strongholdId, out stronghold))
            {
                ReplyError(session, packet, Error.StrongholdNotFound);
                return;
            }

            locker.Lock(stronghold).Do(() =>
            {
                PacketHelper.AddStrongholdProfileToPacket(session, stronghold, reply);

                session.Write(reply);
            });
        }

        private void GetInfoByName(Session session, Packet packet)
        {
            var reply = new Packet(packet);
            string name;
            try
            {
                name = packet.GetString();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            IStronghold stronghold;
            if (!strongholdManager.TryGetStronghold(name, out stronghold))
            {
                ReplyError(session, packet, Error.StrongholdNotFound);
                return;
            }

            locker.Lock(stronghold).Do(() =>
            {
                if (stronghold == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                PacketHelper.AddStrongholdProfileToPacket(session, stronghold, reply);

                session.Write(reply);
            });
        }

        private void GetName(Session session, Packet packet)
        {
            var reply = new Packet(packet);

            byte count;
            uint[] strongholdIds;
            try
            {
                count = packet.GetByte();
                strongholdIds = new uint[count];
                for (int i = 0; i < count; i++)
                {
                    strongholdIds[i] = packet.GetUInt32();
                }
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            reply.AddByte(count);

            for (int i = 0; i < count; i++)
            {
                uint strongholdId = strongholdIds[i];
                IStronghold stronghold;
                if (!strongholdManager.TryGetStronghold(strongholdIds[i], out stronghold))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                reply.AddUInt32(strongholdId);
                reply.AddString(stronghold.Name);
            }

            session.Write(reply);
        }

        private void GateRepair(Session session, Packet packet)
        {
            uint strongholdId;

            try
            {
                strongholdId = packet.GetUInt32();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            if (session.Player.Tribesman == null)
            {
                ReplyError(session, packet, Error.TribeIsNull);
                return;
            }
            var tribe = session.Player.Tribesman.Tribe;

            IStronghold stronghold;
            if (!strongholdManager.TryGetStronghold(strongholdId, out stronghold))
            {
                ReplyError(session, packet, Error.StrongholdNotFound);
                return;
            }

            locker.Lock(tribe, stronghold).Do(() =>
            {
                if (!stronghold.BelongsTo(tribe))
                {
                    ReplyError(session, packet, Error.StrongholdNotOccupied);
                    return;
                }

                if (!tribe.HasRight(session.Player.PlayerId, TribePermission.Repair))
                {
                    ReplyError(session, packet, Error.TribesmanNotAuthorized);
                    return;
                }

                var result = strongholdManager.RepairGate(stronghold);
                ReplyWithResult(session, packet, result);
            });
        }

        private void SetTheme(Session session, Packet packet)
        {
            uint strongholdId;
            string themeId;

            try
            {
                strongholdId = packet.GetUInt32();
                themeId = packet.GetString();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            if (session.Player.Tribesman == null)
            {
                ReplyError(session, packet, Error.TribeIsNull);
                return;
            }
            var tribe = session.Player.Tribesman.Tribe;

            IStronghold stronghold;
            if (!strongholdManager.TryGetStronghold(strongholdId, out stronghold))
            {
                ReplyError(session, packet, Error.StrongholdNotFound);
                return;
            }

            locker.Lock(tribe, stronghold).Do(() =>
            {
                if (!stronghold.BelongsTo(tribe))
                {
                    ReplyError(session, packet, Error.StrongholdNotOccupied);
                    return;
                }

                if (!tribe.HasRight(session.Player.PlayerId, TribePermission.SetStrongholdTheme))
                {
                    ReplyError(session, packet, Error.TribesmanNotAuthorized);
                    return;
                }

                var result = themeManager.SetStrongholdTheme(stronghold, session.Player, themeId);
                ReplyWithResult(session, packet, result);
            });
        }
    }
}
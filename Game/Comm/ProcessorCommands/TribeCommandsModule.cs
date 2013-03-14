#region

using System;
using System.Linq;
using Game.Data;
using Game.Data.Stronghold;
using Game.Data.Tribe;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util.Locking;

#endregion

namespace Game.Comm.ProcessorCommands
{
    class TribeCommandsModule : CommandModule
    {
        private readonly ILocker locker;

        private readonly Procedure procedure;

        private readonly IStrongholdManager strongholdManager;

        private readonly ITribeFactory tribeFactory;

        private readonly ITribeManager tribeManager;

        private readonly IWorld world;

        public TribeCommandsModule(ITribeFactory tribeFactory,
                                   IStrongholdManager strongholdManager,
                                   IWorld world,
                                   ITribeManager tribeManager,
                                   ILocker locker,
                                   Procedure procedure)
        {
            this.tribeFactory = tribeFactory;
            this.strongholdManager = strongholdManager;
            this.world = world;
            this.tribeManager = tribeManager;
            this.locker = locker;
            this.procedure = procedure;
        }

        public override void RegisterCommands(Processor processor)
        {
            processor.RegisterCommand(Command.TribeNameGet, GetName);
            processor.RegisterCommand(Command.TribeInfo, GetInfo);
            processor.RegisterCommand(Command.TribeCreate, Create);
            processor.RegisterCommand(Command.TribeDelete, Delete);
            processor.RegisterCommand(Command.TribeUpgrade, Upgrade);
            processor.RegisterCommand(Command.TribeSetDescription, SetDescription);
            processor.RegisterCommand(Command.TribeInfoByName, GetInfoByName);
            processor.RegisterCommand(Command.TribeUpdateRank, UpdateRank);
        }

        private void UpdateRank(Session session, Packet packet)
        {
            byte id;
            string name;
            int permission;
            try
            {
                id = packet.GetByte();
                name = packet.GetString();
                permission = packet.GetInt32();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            var tribe = session.Player.Tribesman != null ? session.Player.Tribesman.Tribe : null;

            if (tribe == null)
            {
                ReplyError(session, packet, Error.TribeIsNull);
                return;
            }

            using (locker.Lock(session.Player,tribe))
            {
                if (!session.Player.Tribesman.Tribe.HasRight(session.Player.PlayerId, TribePermission.All))
                {
                    ReplyError(session, packet, Error.TribesmanNotAuthorized);
                    return;
                }
                ReplyWithResult(session, packet, tribe.UpdateRank(id, name, (TribePermission)permission));
            }
        }

        private void SetDescription(Session session, Packet packet)
        {
            string description;
            string publicDescription;
            try
            {
                description = packet.GetString();
                publicDescription = packet.GetString();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            using (locker.Lock(session.Player))
            {
                if (session.Player.Tribesman == null)
                {
                    ReplyError(session, packet, Error.TribeIsNull);
                    return;
                }

                if (!session.Player.Tribesman.Tribe.IsOwner(session.Player))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                if (description.Length > Player.MAX_DESCRIPTION_LENGTH || publicDescription.Length > Player.MAX_DESCRIPTION_LENGTH)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                session.Player.Tribesman.Tribe.Description = description;
                session.Player.Tribesman.Tribe.PublicDescription = publicDescription;

                ReplySuccess(session, packet);
            }
        }

        private void GetName(Session session, Packet packet)
        {
            var reply = new Packet(packet);

            byte count;
            uint[] tribeIds;
            try
            {
                count = packet.GetByte();
                tribeIds = new uint[count];
                for (int i = 0; i < count; i++)
                {
                    tribeIds[i] = packet.GetUInt32();
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
                uint tribeId = tribeIds[i];
                ITribe tribe;

                if (!world.TryGetObjects(tribeId, out tribe))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                reply.AddUInt32(tribeId);
                reply.AddString(tribe.Name);
            }

            session.Write(reply);
        }

        private void GetInfo(Session session, Packet packet)
        {
            var reply = new Packet(packet);
            uint id;
            try
            {
                id = packet.GetUInt32();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            ITribe tribe;

            using (locker.Lock(id, out tribe))
            {
                if (tribe == null)
                {
                    ReplyError(session, packet, Error.TribeNotFound);
                    return;
                }

                PacketHelper.AddTribeInfo(strongholdManager, tribeManager, session, tribe, reply);

                session.Write(reply);
            }
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

            uint id;
            if (!tribeManager.FindTribeId(name, out id))
            {
                ReplyError(session, packet, Error.TribeNotFound);
                return;
            }

            ITribe tribe;
            using (locker.Lock(id, out tribe))
            {
                if (tribe == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                PacketHelper.AddTribeInfo(strongholdManager, tribeManager, session, tribe, reply);

                session.Write(reply);
            }
        }

        private void Create(Session session, Packet packet)
        {
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

            using (locker.Lock(session.Player))
            {
                if (!session.Player.GetCityList().Any(city => city.Lvl >= 5))
                {
                    ReplyError(session, packet, Error.EffectRequirementNotMet);
                }

                ITribe tribe;
                Error error = procedure.CreateTribe(session.Player, name, out tribe);
                if (error == Error.Ok)
                {
                    var reply = new Packet(packet);
                    PacketHelper.AddTribeRanksToPacket(tribe, reply);
                    session.Write(reply);
                } 
                else
                {
                    ReplyError(session, packet, error);
                }
            }
        }

        private void Delete(Session session, Packet packet)
        {
            if (!session.Player.IsInTribe)
            {
                ReplyError(session, packet, Error.TribeIsNull);
                return;
            }

            ITribe tribe = session.Player.Tribesman.Tribe;

            CallbackLock.CallbackLockHandler lockHandler = delegate
                {
                    var locks =
                            strongholdManager.StrongholdsForTribe(tribe)
                                             .SelectMany(stronghold => stronghold.LockList)
                                             .ToList();

                    locks.AddRange(tribe.Tribesmen);

                    return locks.ToArray();
                };

            using (locker.Lock(lockHandler, new object[] {}, tribe))
            {
                if (!session.Player.Tribesman.Tribe.IsOwner(session.Player))
                {
                    ReplyError(session, packet, Error.TribesmanNotAuthorized);
                    return;
                }

                var result = tribeManager.Remove(tribe);
                ReplyWithResult(session, packet, result);
            }
        }

        private void Upgrade(Session session, Packet packet)
        {
            if (!session.Player.Tribesman.Tribe.IsOwner(session.Player))
            {
                ReplyError(session, packet, Error.TribesmanNotAuthorized);
                return;
            }

            ITribe tribe = session.Player.Tribesman.Tribe;
            using (locker.Lock(tribe))
            {
                var result = tribe.Upgrade();
                ReplyWithResult(session, packet, result);
            }
        }
    }
}
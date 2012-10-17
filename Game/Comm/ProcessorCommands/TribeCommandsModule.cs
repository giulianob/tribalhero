#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Data.Stronghold;
using Game.Data.Tribe;
using Game.Database;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util;
using System.Linq;
using Game.Util.Locking;

#endregion

namespace Game.Comm.ProcessorCommands
{
    class TribeCommandsModule : CommandModule
    {
        private readonly ITribeFactory tribeFactory;
        private readonly IStrongholdManager strongholdManager;

        public TribeCommandsModule(ITribeFactory tribeFactory, IStrongholdManager strongholdManager)
        {
            this.tribeFactory = tribeFactory;
            this.strongholdManager = strongholdManager;
        }

        public override void RegisterCommands(Processor processor)
        {
            processor.RegisterCommand(Command.TribeNameGet, GetName);
            processor.RegisterCommand(Command.TribeInfo, GetInfo);
            processor.RegisterCommand(Command.TribeCreate, Create);
            processor.RegisterCommand(Command.TribeDelete, Delete);
            processor.RegisterCommand(Command.TribeUpgrade, Upgrade);
            processor.RegisterCommand(Command.TribeSetDescription, SetDescription);
            processor.RegisterCommand(Command.TribePublicInfo, GetPublicInfo);
            processor.RegisterCommand(Command.TribeInfoByName, GetInfoByName);
        }

        private void SetDescription(Session session, Packet packet)
        {
            string description;
            try
            {
                description = packet.GetString();
            }
            catch (Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            using (Concurrency.Current.Lock(session.Player))
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

                if (description.Length > Player.MAX_DESCRIPTION_LENGTH)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                session.Player.Tribesman.Tribe.Description = description;

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
                    tribeIds[i] = packet.GetUInt32();
            }
            catch (Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            reply.AddByte(count);
            for (int i = 0; i < count; i++)
            {
                uint tribeId = tribeIds[i];
                ITribe tribe;

                if (!World.Current.TryGetObjects(tribeId, out tribe))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                reply.AddUInt32(tribeId);
                reply.AddString(tribe.Name);
            }

            session.Write(reply);
        }

        private void AddPrivateInfo(ITribe tribe, Packet packet)
        {
            packet.AddUInt32(tribe.Id);
            packet.AddUInt32(tribe.Owner.PlayerId);
            packet.AddByte(tribe.Level);
            packet.AddString(tribe.Name);
            packet.AddString(tribe.Description);
            packet.AddFloat((float)tribe.VictoryPoint);
            PacketHelper.AddToPacket(tribe.Resource, packet);

            packet.AddInt16((short)tribe.Count);
            foreach (var tribesman in tribe.Tribesmen)
            {
                packet.AddUInt32(tribesman.Player.PlayerId);
                packet.AddString(tribesman.Player.Name);
                packet.AddInt32(tribesman.Player.GetCityCount());
                packet.AddByte(tribesman.Rank);
                packet.AddUInt32(tribesman.Player.IsLoggedIn ? 0 : UnixDateTime.DateTimeToUnix(tribesman.Player.LastLogin));
                PacketHelper.AddToPacket(tribesman.Contribution, packet);
            }

            // Incoming List
            var incomingList = tribe.GetIncomingList().ToList();
            packet.AddInt16((short)incomingList.Count());
            foreach (var incoming in incomingList)
            {
                // Target
                packet.AddUInt32(incoming.TargetCity.Owner.PlayerId);
                packet.AddUInt32(incoming.TargetCity.Id);
                packet.AddString(incoming.TargetCity.Owner.Name);
                packet.AddString(incoming.TargetCity.Name);

                // Attacker
                packet.AddUInt32(incoming.SourceCity.Owner.PlayerId);
                packet.AddUInt32(incoming.SourceCity.Id);
                packet.AddString(incoming.SourceCity.Owner.Name);
                packet.AddString(incoming.SourceCity.Name);

                packet.AddUInt32(UnixDateTime.DateTimeToUnix(incoming.EndTime.ToUniversalTime()));
            }

            // Assignment List
            packet.AddInt16(tribe.AssignmentCount);
            foreach (var assignment in tribe.Assignments)
            {
                PacketHelper.AddToPacket(assignment, packet);
            }

            // Strongholds
            var strongholds = strongholdManager.Where(x => x.Tribe != null && x.Tribe.Id == tribe.Id).ToList();
            packet.AddInt16((short)strongholds.Count());
            foreach (var stronghold in strongholds)
            {
                packet.AddUInt32(stronghold.Id);
                packet.AddString(stronghold.Name);
                packet.AddByte((byte)stronghold.StrongholdState);
                packet.AddByte(stronghold.Lvl);
                packet.AddFloat((float)stronghold.Gate);
                packet.AddUInt32(stronghold.X);
                packet.AddUInt32(stronghold.Y);
                packet.AddInt32(stronghold.Troops.StationedHere().Sum(x=> x.Upkeep));
                packet.AddFloat((float)stronghold.VictoryPointRate);
                packet.AddUInt32(UnixDateTime.DateTimeToUnix(stronghold.DateOccupied.ToUniversalTime()));
                packet.AddUInt32(stronghold.GateOpenTo == null ? 0 : stronghold.GateOpenTo.Id);
                PacketHelper.AddToPacket(stronghold.State, packet);
            }
        }
        private void AddPublicInfo(ITribe tribe, Packet packet)
        {
            packet.AddUInt32(tribe.Id);
            packet.AddString(tribe.Name);
            packet.AddInt16((short)tribe.Count);
            foreach (var tribesman in tribe.Tribesmen)
            {
                packet.AddUInt32(tribesman.Player.PlayerId);
                packet.AddString(tribesman.Player.Name);
                packet.AddInt32(tribesman.Player.GetCityCount());
                packet.AddByte(tribesman.Rank);
            }

        }
        private void GetInfo(Session session, Packet packet)
        {
            var reply = new Packet(packet);
            if (session.Player.Tribesman == null)
            {
                ReplyError(session, packet, Error.TribeIsNull);
                return;
            }
            var tribe = session.Player.Tribesman.Tribe;

            using (Concurrency.Current.Lock(tribe))
            {
                if (tribe == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }
                AddPrivateInfo(tribe, reply);
                session.Write(reply);
            }
        }

        private void GetPublicInfo(Session session, Packet packet)
        {
            var reply = new Packet(packet);
            uint id;
            try
            {
                id = packet.GetUInt32();
            }
            catch (Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            ITribe tribe;

            using (Concurrency.Current.Lock(id, out tribe))
            {
                if (tribe == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }
                AddPublicInfo(tribe, reply);
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
            catch (Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            uint id;
            if(!World.Current.FindTribeId(name,out id))
            {
                ReplyError(session, packet, Error.TribeNotFound);
                return;
            }
            
            ITribe tribe;
            using (Concurrency.Current.Lock(id, out tribe))
            {
                if (tribe == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }
                if(session.Player.IsInTribe && tribe.Id == session.Player.Tribesman.Tribe.Id)
                {
                    reply.AddByte(1);
                    AddPrivateInfo(tribe, reply);
                }
                else
                {
                    reply.AddByte(0);
                    AddPublicInfo(tribe, reply);
                }
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
            catch (Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            using (Concurrency.Current.Lock(session.Player))
            {
                if (session.Player.Tribesman != null)
                {
                    ReplyError(session, packet, Error.TribesmanAlreadyInTribe);
                    return;
                }

                if (World.Current.TribeNameTaken(name))
                {
                    ReplyError(session, packet, Error.TribeAlreadyExists);
                    return;
                }

                if (!Tribe.IsNameValid(name))
                {
                    ReplyError(session, packet, Error.TribeNameInvalid);
                    return;
                }

                if (!session.Player.GetCityList().Any(city => city.Lvl >= 5))
                {
                    ReplyError(session, packet, Error.EffectRequirementNotMet);
                    return;
                }

                ITribe tribe = tribeFactory.CreateTribe(session.Player, name);
                
                World.Current.Add(tribe);

                var tribesman = new Tribesman(tribe, session.Player, 0);
                tribe.AddTribesman(tribesman);

                Global.Channel.Subscribe(session, "/TRIBE/" + tribe.Id);
                ReplySuccess(session, packet);
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
            using (Concurrency.Current.Lock(custom => tribe.Tribesmen.ToArray(), new object[] { }, tribe))
            {
                if (!session.Player.Tribesman.Tribe.IsOwner(session.Player))
                {
                    ReplyError(session, packet, Error.TribesmanNotAuthorized);
                    return;
                }

                if (tribe.AssignmentCount > 0)
                {
                    ReplyError(session, packet, Error.TribeHasAssignment);
                    return;
                }

                foreach (var tribesman in new List<ITribesman>(tribe.Tribesmen))
                {
                    if (tribesman.Player.Session != null)
                        Procedure.Current.OnSessionTribesmanQuit(tribesman.Player.Session, tribe.Id, tribesman.Player.PlayerId, true);
                    tribe.RemoveTribesman(tribesman.Player.PlayerId);
                }

                World.Current.Remove(tribe);                
            }

            ReplySuccess(session, packet);
        }

        private void Upgrade(Session session, Packet packet)
        {
            if (!session.Player.Tribesman.Tribe.IsOwner(session.Player))
            {
                ReplyError(session, packet, Error.TribesmanNotAuthorized);
                return;
            }

            ITribe tribe = session.Player.Tribesman.Tribe;
            using (Concurrency.Current.Lock(tribe))
            {
                if (tribe.Level >= 20)
                {
                    ReplyError(session, packet, Error.TribeMaxLevel);
                    return;
                }
                Resource cost = Formula.Current.GetTribeUpgradeCost(tribe.Level);
                if (!tribe.Resource.HasEnough(cost))
                {
                    ReplyError(session, packet, Error.ResourceNotEnough);
                    return;
                }

                tribe.Upgrade();
            }

            ReplySuccess(session, packet);
        }
    }
}
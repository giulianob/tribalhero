#region

using System;
using Game.Data.Stronghold;
using Game.Logic.Formulas;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Persistance;

#endregion

namespace Game.Comm.ProcessorCommands
{
    class StrongholdCommandsModule : CommandModule
    {
        private readonly IStrongholdManager strongholdManager;
        private readonly IDbManager dbManager;
        private readonly Formula formula;

        public StrongholdCommandsModule(IStrongholdManager strongholdManager, Formula formula, IDbManager dbManager)
        {
            this.strongholdManager = strongholdManager;
            this.formula = formula;
            this.dbManager = dbManager;
        }

        public override void RegisterCommands(Processor processor)
        {
            processor.RegisterCommand(Command.StrongholdNameGet, GetName);
            processor.RegisterCommand(Command.StrongholdInfo, GetInfo);
            processor.RegisterCommand(Command.StrongholdPublicInfo, GetPublicInfo);
            processor.RegisterCommand(Command.StrongholdInfoByName, GetInfoByName);
            processor.RegisterCommand(Command.StrongholdGateRepair, GateRepair);
            processor.RegisterCommand(Command.StrongholdLocate, Locate);
            processor.RegisterCommand(Command.StrongholdLocateByName, LocateByName);
        }

        private void AddPrivateInfo(IStronghold stronghold, Packet packet)
        {
            packet.AddUInt32(stronghold.Id);
            packet.AddString(stronghold.Name);
            packet.AddByte(stronghold.Lvl);
            packet.AddFloat((float)stronghold.Gate);
            packet.AddFloat((float)stronghold.VictoryPointRate);
            packet.AddUInt32(UnixDateTime.DateTimeToUnix(stronghold.DateOccupied.ToUniversalTime()));
            packet.AddUInt32(stronghold.X);
            packet.AddUInt32(stronghold.Y);
            PacketHelper.AddToPacket(stronghold.State, packet);

            packet.AddByte(stronghold.Troops.Size);
            foreach (var troop in stronghold.Troops)
            {
                packet.AddUInt32(troop.City.Owner.PlayerId);
                packet.AddUInt32(troop.City.Id);
                packet.AddString(troop.City.Owner.Name);
                packet.AddString(troop.City.Name);
                packet.AddByte(troop.TroopId);

                //Actual formation and unit counts
                packet.AddByte(troop.FormationCount);
                foreach (var formation in troop)
                {
                    packet.AddByte((byte)formation.Type);
                    packet.AddByte((byte)formation.Count);
                    foreach (var kvp in formation)
                    {
                        packet.AddUInt16(kvp.Key);
                        packet.AddUInt16(kvp.Value);
                    }
                }
            }

            // Incoming List
            // Reports
        }

        private void AddPublicInfo(IStronghold stronghold, Packet packet)
        {
            packet.AddUInt32(stronghold.Id);
            packet.AddByte((byte)stronghold.State.Type);
            packet.AddByte((byte)stronghold.StrongholdState);
            packet.AddUInt32(stronghold.Tribe == null ? 0 : stronghold.Tribe.Id);
        }

        private void GetPublicInfo(Session session, Packet packet)
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

            IStronghold stronghold;
            if (!strongholdManager.TryGetStronghold(strongholdId, out stronghold))
            {
                ReplyError(session, packet, Error.StrongholdNotFound);
                return;
            }

            using (Concurrency.Current.Lock(stronghold))
            {
                AddPublicInfo(stronghold, reply);
                session.Write(reply);
            }
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
            var tribe = session.Player.Tribesman.Tribe;

            IStronghold stronghold;
            if (!strongholdManager.TryGetStronghold(strongholdId, out stronghold))
            {
                ReplyError(session, packet, Error.StrongholdNotFound);
                return;
            }

            using (Concurrency.Current.Lock(tribe, stronghold))
            {
                if (stronghold.StrongholdState != StrongholdState.Occupied || tribe.Id != stronghold.Tribe.Id)
                {
                    ReplyError(session, packet, Error.StrongholdNotOccupied);
                    return;
                }
                AddPrivateInfo(stronghold, reply);
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

            IStronghold stronghold;
            if (!strongholdManager.TryGetStronghold(name, out stronghold))
            {
                ReplyError(session, packet, Error.StrongholdNotFound);
                return;
            }

            using (Concurrency.Current.Lock(stronghold))
            {
                if (stronghold == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }
                if (session.Player.IsInTribe && stronghold.StrongholdState == StrongholdState.Occupied &&
                    stronghold.Tribe.Id == session.Player.Tribesman.Tribe.Id)
                {
                    reply.AddByte(1);
                    AddPrivateInfo(stronghold, reply);
                }
                else
                {
                    reply.AddByte(0);
                    AddPublicInfo(stronghold, reply);
                }
                session.Write(reply);
            }

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
                    strongholdIds[i] = packet.GetUInt32();
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
            catch (Exception)
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

            using (Concurrency.Current.Lock(tribe, stronghold))
            {
                if (stronghold.StrongholdState != StrongholdState.Occupied || tribe.Id != stronghold.Tribe.Id)
                {
                    ReplyError(session, packet, Error.StrongholdNotOccupied);
                    return;
                }

                if (!tribe.HasRight(session.Player.PlayerId, "Repair"))
                {
                    ReplyError(session, packet, Error.TribesmanNotAuthorized);
                    return;
                }

                var diff = formula.GetGateLimit(stronghold.Lvl) - stronghold.Gate;
                if(diff<=0)
                {
                    ReplyError(session, packet, Error.StrongholdGateFull);
                    return;
                }

                var cost = formula.GetGateRepairCost(stronghold.Lvl, diff);
                if(!tribe.Resource.HasEnough(cost))
                {
                    ReplyError(session, packet, Error.ResourceNotEnough);
                    return;
                }
                tribe.Resource.Subtract(cost);
                dbManager.Save(tribe);
                
                stronghold.BeginUpdate();
                stronghold.Gate = formula.GetGateLimit(stronghold.Lvl);
                stronghold.EndUpdate();
                ReplySuccess(session, packet);
            }
        }

        private void Locate(Session session, Packet packet)
        {
            var reply = new Packet(packet);

            uint strongholdId;

            try
            {
                strongholdId = packet.GetUInt32();
            }
            catch (Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            IStronghold stronghold;
            if (!strongholdManager.TryGetStronghold(strongholdId, out stronghold))
            {
                ReplyError(session, packet, Error.StrongholdNotFound);
                return;
            }

            using (Concurrency.Current.Lock(stronghold))
            {
                if (stronghold == null)
                {
                    ReplyError(session, packet, Error.StrongholdNotFound);
                    return;
                }

                reply.AddUInt32(stronghold.X);
                reply.AddUInt32(stronghold.Y);
                session.Write(reply);
            }
        }

        private void LocateByName(Session session, Packet packet)
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

            IStronghold stronghold;
            if (!strongholdManager.TryGetStronghold(name, out stronghold))
            {
                ReplyError(session, packet, Error.StrongholdNotFound);
                return;
            }


            using (Concurrency.Current.Lock(stronghold))
            {
                if (stronghold == null)
                {
                    ReplyError(session, packet, Error.CityNotFound);
                    return;
                }

                reply.AddUInt32(stronghold.X);
                reply.AddUInt32(stronghold.Y);

                session.Write(reply);
            }
        }


    }
}
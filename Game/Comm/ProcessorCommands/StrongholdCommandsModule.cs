#region

using System;
using Game.Data.Stronghold;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;

#endregion

namespace Game.Comm.ProcessorCommands
{
    class StrongholdCommandsModule : CommandModule
    {
        private readonly IStrongholdManager strongholdManager;

        public StrongholdCommandsModule(IStrongholdManager strongholdManager)
        {
            this.strongholdManager = strongholdManager;
        }

        public override void RegisterCommands(Processor processor)
        {
            processor.RegisterCommand(Command.StrongholdNameGet, GetName);
            processor.RegisterCommand(Command.StrongholdInfo, GetInfo);
            processor.RegisterCommand(Command.StrongholdPublicInfo, GetPublicInfo);
            processor.RegisterCommand(Command.StrongholdInfoByName, GetInfoByName);
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
            if(!strongholdManager.TryGetStronghold(strongholdId, out stronghold))
            {
                ReplyError(session, packet, Error.StrongholdNotFound);
                return;
            }

            using (Concurrency.Current.Lock(tribe,stronghold))
            {
                if(stronghold.StrongholdState!= StrongholdState.Occupied || tribe.Id != stronghold.Tribe.Id)
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
            catch (Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            IStronghold stronghold;
            if (!strongholdManager.TryGetStronghold(name, out stronghold))
            {
                ReplyError(session, packet, Error.TribeNotFound);
                return;
            }

            using (Concurrency.Current.Lock(stronghold))
            {
                if (stronghold == null)
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }
                if (session.Player.IsInTribe && stronghold.StrongholdState==StrongholdState.Occupied && stronghold.Tribe.Id == session.Player.Tribesman.Tribe.Id)
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
            catch (Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            reply.AddByte(count);

            for (int i = 0; i < count; i++)
            {
                uint strongholdId = strongholdIds[i];
                IStronghold stronghold;
                if (!strongholdManager.TryGetStronghold(strongholdIds[i],out stronghold))
                {
                    ReplyError(session, packet, Error.Unexpected);
                    return;
                }

                reply.AddUInt32(strongholdId);
                reply.AddString(stronghold.Name);
            }

            session.Write(reply);
        }

    }
}
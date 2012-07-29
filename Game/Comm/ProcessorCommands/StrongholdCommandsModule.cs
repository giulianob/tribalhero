#region

using System;
using Game.Data;
using Game.Data.Stronghold;
using Game.Data.Tribe;
using Game.Map;
using Game.Setup;
using Game.Util;
using System.Linq;
using Game.Util.Locking;
using Ninject;

#endregion

namespace Game.Comm.ProcessorCommands
{
    class StrongholdCommandsModule : CommandModule
    {
        public override void RegisterCommands(Processor processor)
        {
            processor.RegisterCommand(Command.StrongholdNameGet, GetName);
            processor.RegisterCommand(Command.StrongholdInfo, GetInfo);
            processor.RegisterCommand(Command.StrongholdPublicInfo, GetPublicInfo);
        }

        private void GetPublicInfo(Session session, Packet packet)
        {
            throw new NotImplementedException();
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

            IStrongholdManager strongholdManager = Ioc.Kernel.Get<IStrongholdManager>();
            IStronghold stronghold;
            if(!strongholdManager.TryGetValue(strongholdId, out stronghold))
            {
                ReplyError(session, packet, Error.StrongholdNotFound);
                return;
            }

            using (Concurrency.Current.Lock(tribe,stronghold))
            {
                if(stronghold.StrongholdState!= StrongholdState.Occupied || tribe.Id == stronghold.Tribe.Id)
                {
                    ReplyError(session, packet, Error.StrongholdNotOccupied);
                    return;
                }

                reply.AddUInt32(stronghold.Id);
                reply.AddInt32(stronghold.Gate.Value);
                reply.AddByte((byte)stronghold.State.Type);
                reply.AddByte(stronghold.Troops.Size);
                foreach(var troop in stronghold.Troops)
                {
                    PacketHelper.AddToPacket(troop, packet);
                }

                // Incoming List
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

            IStrongholdManager strongholdManager = Ioc.Kernel.Get<IStrongholdManager>();

            for (int i = 0; i < count; i++)
            {
                uint strongholdId = strongholdIds[i];
                IStronghold stronghold;
                if (!strongholdManager.TryGetValue(strongholdIds[i],out stronghold))
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
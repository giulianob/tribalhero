#region

using System;
using Game.Data.Stronghold;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;

#endregion

namespace Game.Comm.ProcessorCommands
{
    class StrongholdCommandsModule : CommandModule
    {
        private readonly IStrongholdManager strongholdManager;

        private readonly IWorld world;

        public StrongholdCommandsModule(IStrongholdManager strongholdManager, IWorld world)
        {
            this.strongholdManager = strongholdManager;
            this.world = world;
        }

        public override void RegisterCommands(Processor processor)
        {
            processor.RegisterCommand(Command.StrongholdNameGet, GetName);
            processor.RegisterCommand(Command.StrongholdInfo, GetInfo);
            processor.RegisterCommand(Command.StrongholdInfoByName, GetInfoByName);
            processor.RegisterCommand(Command.StrongholdLocate, Locate);
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
            using (Concurrency.Current.Lock(strongholdId, out stronghold))
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
            
            IStronghold stronghold;
            if(!strongholdManager.TryGetStronghold(strongholdId, out stronghold))
            {
                ReplyError(session, packet, Error.StrongholdNotFound);
                return;
            }

            using (Concurrency.Current.Lock(stronghold))
            {
                PacketHelper.AddStrongholdProfileToPacket(session, stronghold, reply);

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

                PacketHelper.AddStrongholdProfileToPacket(session, stronghold, reply);

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
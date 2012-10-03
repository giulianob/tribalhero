using System;
using Game.Data;
using Game.Data.Stronghold;
using Game.Data.Tribe;
using Game.Setup;
using Game.Util.Locking;

namespace Game.Comm.ProcessorCommands
{
    class ProfileCommandsModule : CommandModule
    {
        private readonly ILocker locker;

        public ProfileCommandsModule(ILocker locker)
        {
            this.locker = locker;
        }

        public override void RegisterCommands(Processor processor)
        {
            processor.RegisterCommand(Command.ProfileByType, ProfileByType);
        }
    
        //TODO: Add stronghold support and move to where it makes sense
        private void ProfileByType(Session session, Packet packet)
        {
            var reply = new Packet(packet);

            string type;
            uint id;
            try
            {
                type = packet.GetString().ToLowerInvariant();
                id = packet.GetUInt32();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            if (type == "city")
            {
                ICity city;
                using (locker.Lock(id, out city))
                {
                    if (city == null)
                    {
                        ReplyError(session, reply, Error.PlayerNotFound);
                        return;
                    }

                    PacketHelper.AddPlayerProfileToPacket(city.Owner, reply);

                    session.Write(reply);
                }
            }
            else if (type == "stronghold")
            {
                IStronghold stronghold;
                using (locker.Lock(id, out stronghold))
                {
                    if (stronghold == null)
                    {
                        ReplyError(session, reply, Error.ObjectNotFound);
                        return;                        
                    }

                    PacketHelper.AddStrongholdProfileToPacket(session, stronghold, reply);

                    session.Write(reply);
                }
            }
            else
            {
                ReplyError(session, packet, Error.Unexpected);
            }
        }
    }
}

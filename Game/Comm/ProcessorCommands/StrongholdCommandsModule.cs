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
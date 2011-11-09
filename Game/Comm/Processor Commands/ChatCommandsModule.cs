using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.Comm.Processor_Commands
{
    class ChatCommandsModule : CommandModule
    {
        private ITcpServer tcpServer;

        public ChatCommandsModule(ITcpServer tcpServer)
        {
            this.tcpServer = tcpServer;
        }

        public override void RegisterCommands(Processor processor)
        {
            throw new NotImplementedException();
        }

        private void Chat(Session session, Packet packet)
        {

        }
    }
}

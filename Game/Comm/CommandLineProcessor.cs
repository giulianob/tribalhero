using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.Comm
{
    class CommandLineProcessor
    {
        private class ProcessorCommand
        {
            public DoWork Function { get; private set; }

            public ProcessorCommand(DoWork function)
            {
                Function = function;
            }
        }
        
        public delegate void DoWork(Session session, Packet packet);

        readonly Dictionary<CmdLineCommands, ProcessorCommand> commands = new Dictionary<CmdLineCommands, ProcessorCommand>();

        public CommandLineProcessor() {
            
        }

        protected void RegisterCommand(CmdLineCommands cmd, DoWork func)
        {
            commands[cmd] = new ProcessorCommand(func);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Util;

namespace Game.Comm
{
    public partial class CmdLineProcessor
    {
        private class ProcessorCommand
        {
            public DoWork Function { get; private set; }

            public ProcessorCommand(DoWork function)
            {
                Function = function;
            }
        }

        public delegate string DoWork(string[] parms);

        readonly Dictionary<CmdLineCommand, ProcessorCommand> commands = new Dictionary<CmdLineCommand, ProcessorCommand>();

        public CmdLineProcessor()
        {
            RegisterCommand(CmdLineCommand.SEND_RESOURCES, CmdSendResources);
        }

        protected void RegisterCommand(CmdLineCommand cmd, DoWork func)
        {
            commands[cmd] = new ProcessorCommand(func);
        }

        public string Execute(string cmd, string parms)
        {

            if (parms == null) parms = string.Empty;

            CmdLineCommand cmdCode;

            try
            {
                cmdCode = (CmdLineCommand)Enum.Parse(typeof(CmdLineCommand), cmd, true);
            }
            catch (Exception)
            {
                return "Command not found";
            }

            ProcessorCommand cmdWorker;
            return !commands.TryGetValue(cmdCode, out cmdWorker) ? "Command not registered" : commands[cmdCode].Function(CmdParserExtension.SplitCommandLine(parms).ToArray());
        }        
    }
}
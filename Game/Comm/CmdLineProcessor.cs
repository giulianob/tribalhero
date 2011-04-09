#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Util;

#endregion

namespace Game.Comm
{
    public partial class CmdLineProcessor
    {
        #region Delegates

        public delegate string DoWork(string[] parms);

        #endregion

        private readonly Dictionary<CmdLineCommand, ProcessorCommand> commands = new Dictionary<CmdLineCommand, ProcessorCommand>();

        public CmdLineProcessor()
        {
            RegisterCommand(CmdLineCommand.Ban, CmdBanPlayer);
            RegisterCommand(CmdLineCommand.Unban, CmdUnbanPlayer);
            RegisterCommand(CmdLineCommand.Delete, CmdDeletePlayer);
            RegisterCommand(CmdLineCommand.SendResources, CmdSendResources);
        }

        protected void RegisterCommand(CmdLineCommand cmd, DoWork func)
        {
            commands[cmd] = new ProcessorCommand(func);
        }

        public string Execute(string cmd, string parms)
        {
            switch(cmd)
            {
                case "?":
                case "h":
                case "help":
                    return GetCommandList();
            }

            if (parms == null)
                parms = string.Empty;

            CmdLineCommand cmdCode;

            try
            {
                cmdCode = (CmdLineCommand)Enum.Parse(typeof(CmdLineCommand), cmd, true);
            }
            catch(Exception)
            {
                return "Command not found";
            }

            ProcessorCommand cmdWorker;
            return !commands.TryGetValue(cmdCode, out cmdWorker)
                           ? "Command not registered"
                           : commands[cmdCode].Function(CmdParserExtension.SplitCommandLine(parms).ToArray());
        }

        private string GetCommandList()
        {
            var sb = new StringBuilder();
            sb.Append("Commands available:\n\n");
            foreach (var cmd in commands.Keys)
            {
                sb.Append(cmd.ToString().ToLower());
                sb.Append("\n");
            }
            sb.Append("\nType \"command_name h\" for more information about a specific command\n");
            return sb.ToString();
        }

        #region Nested type: ProcessorCommand

        private class ProcessorCommand
        {
            public ProcessorCommand(DoWork function)
            {
                Function = function;
            }

            public DoWork Function { get; private set; }
        }

        #endregion
    }
}
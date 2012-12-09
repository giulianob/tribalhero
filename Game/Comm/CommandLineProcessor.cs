#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Game.Data;
using Game.Util;

#endregion

namespace Game.Comm
{
    public class CommandLineProcessor
    {
        public delegate string DoWork(Session session, string[] parms);

        private readonly Dictionary<string, ProcessorCommand> commands = new Dictionary<string, ProcessorCommand>();

        private readonly StreamWriter writer;

        public CommandLineProcessor(StreamWriter writer, params CommandLineModule[] modules)
        {
            this.writer = writer;

            foreach (CommandLineModule module in modules)
            {
                module.RegisterCommands(this);
            }
        }

        public void RegisterCommand(string command, DoWork func, PlayerRights rightsRequired)
        {
            commands[command.ToLower()] = new ProcessorCommand(func, rightsRequired);
        }

        public string Execute(Session session, string cmd, string parms)
        {
            switch(cmd)
            {
                case "?":
                case "h":
                case "help":
                    return GetCommandList(session.Player.Rights);
            }

            if (parms == null)
            {
                parms = string.Empty;
            }

            ProcessorCommand cmdWorker;
            if (!commands.TryGetValue(cmd, out cmdWorker) || cmdWorker.RightsRequired > session.Player.Rights)
            {
                return "Command not found";
            }

            var result = cmdWorker.Function(session, CmdParserExtension.SplitCommandLine(parms).ToArray());

            if (cmdWorker.RightsRequired > PlayerRights.Basic)
            {
                writer.WriteLine("({4}) {0}: '{1} {2}' >>>> {3}", session.Player.Name, cmd, parms, result, DateTime.Now);
            }

            return result;
        }

        private string GetCommandList(PlayerRights rights)
        {
            var sb = new StringBuilder();
            sb.Append("Commands available:\n\n");
            foreach (var cmd in commands.Where(cmd => cmd.Value.RightsRequired <= rights))
            {
                sb.Append(cmd.Key.ToLower());
                sb.Append("\n");
            }
            sb.Append("\nType \"command_name h\" for more information about a specific command\n");
            return sb.ToString();
        }

        #region Nested type: ProcessorCommand

        private class ProcessorCommand
        {
            public ProcessorCommand(DoWork function, PlayerRights rightsRequired)
            {
                Function = function;
                RightsRequired = rightsRequired;
            }

            public DoWork Function { get; private set; }

            public PlayerRights RightsRequired { get; private set; }
        }

        #endregion
    }
}
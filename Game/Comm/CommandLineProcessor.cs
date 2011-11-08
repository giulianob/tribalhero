#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Util;

#endregion

namespace Game.Comm
{
    public class CommandLineProcessor
    {        
        public delegate string DoWork(Session session, string[] parms);
        
        private readonly Dictionary<string, ProcessorCommand> commands = new Dictionary<string, ProcessorCommand>();

        public CommandLineProcessor(params CommandLineModule[] modules)
        {
            foreach (CommandLineModule module in modules)
            {
                module.RegisterCommands(this);
            }
        }

        public void RegisterCommand(string command, DoWork func, bool adminOnly)
        {
            commands[command.ToLower()] = new ProcessorCommand(func, adminOnly);
        }

        public string Execute(Session session, string cmd, string parms)
        {
            switch(cmd)
            {
                case "?":
                case "h":
                case "help":
                    return GetCommandList(session.Player.Admin);
            }

            if (parms == null)
                parms = string.Empty;

            ProcessorCommand cmdWorker;
            if (!commands.TryGetValue(cmd, out cmdWorker) || (!session.Player.Admin && cmdWorker.AdminOnly))
            {
                return "Command not found";
            }

            return cmdWorker.Function(session,CmdParserExtension.SplitCommandLine(parms).ToArray());
        }

        private string GetCommandList(bool isAdmin)
        {
            var sb = new StringBuilder();
            sb.Append("Commands available:\n\n");
            foreach (var cmd in commands.Where(cmd => isAdmin || !cmd.Value.AdminOnly))
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
            public ProcessorCommand(DoWork function, bool adminOnly)
            {
                Function = function;
                AdminOnly = adminOnly;
            }

            public DoWork Function { get; private set; }

            public bool AdminOnly { get; private set; }
        }

        #endregion
    }
}
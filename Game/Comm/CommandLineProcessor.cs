#region

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data;
using Game.Setup;
using Game.Util;
using Ninject.Extensions.Logging;

#endregion

namespace Game.Comm
{
    public class CommandLineProcessor
    {
        private readonly ILogger logger = LoggerFactory.Current.GetCurrentClassLogger();

        public delegate string DoWork(Session session, string[] parms);

        private readonly Dictionary<string, ProcessorCommand> commands = new Dictionary<string, ProcessorCommand>();

        public CommandLineProcessor(params CommandLineModule[] modules)
        {
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

            if (Config.database_load_players &&
                cmdWorker.RightsRequired > PlayerRights.Basic &&
                cmd != "auth" &&
                (session.Player.HasTwoFactorAuthenticated == null ||
                 SystemClock.Now.Subtract(session.Player.HasTwoFactorAuthenticated.Value).TotalHours > 1))
            {
                return "You must first authenticate as a moderator by using the '/auth' command. If you have not yet set up your account for moderator authentication, visit http://tribalhero.com/mod/players/generate_auth_code";
            }

            var result = cmdWorker.Function(session, CmdParserExtension.SplitCommandLine(parms).ToArray());

            if (cmdWorker.RightsRequired > PlayerRights.Basic)
            {
                logger.Info("{0}: '{1} {2}' >>>> {3}", session.Player.Name, cmd, parms, result);
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
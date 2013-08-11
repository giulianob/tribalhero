#region

using System.Collections.Generic;
using Game.Data;
using Game.Util;
using Ninject.Extensions.Logging;

#endregion

namespace Game.Comm
{
    public class Processor
    {
        #region Delegates

        public delegate void DoWork(Session session, Packet packet);

        #endregion

        private readonly ILogger logger = LoggerFactory.Current.GetCurrentClassLogger();

        private readonly Dictionary<Command, ProcessorCommand> commands = new Dictionary<Command, ProcessorCommand>();

        private readonly Dictionary<Command, ProcessorCommand> events = new Dictionary<Command, ProcessorCommand>();

        public Processor(params CommandModule[] commandModules)
        {
            foreach (CommandModule commandModule in commandModules)
            {
                commandModule.RegisterCommands(this);
            }
        }

        public void RegisterCommand(Command cmd, DoWork func)
        {
            commands[cmd] = new ProcessorCommand(func);
        }

        public void RegisterEvent(Command cmd, DoWork func)
        {
            events[cmd] = new ProcessorCommand(func);
        }

        public void Execute(Session session, Packet packet)
        {
            lock (session)
            {
                ProcessorCommand command;
                if (!commands.TryGetValue(packet.Cmd, out command))
                {
                    return;
                }

                command.Function(session, packet);
            }
        }

        public void ExecuteEvent(Session session, Packet packet)
        {
            lock (session)
            {
                events[packet.Cmd].Function(session, packet);
            }
        }

        #region Nested type: ProcessorCommand

        public class ProcessorCommand
        {
            public readonly DoWork Function;

            public ProcessorCommand(DoWork function)
            {
                Function = function;
            }
        }

        #endregion
    }
}
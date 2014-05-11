using System;
using System.Collections.Generic;
using Common;
using Game.Util;

namespace Game.Comm
{
    public class QueueCommandProcessor : IQueueCommandProcessor
    {
        private readonly ILogger logger = LoggerFactory.Current.GetLogger<QueueCommandProcessor>();

        private readonly Dictionary<string, Action<dynamic>> commands = new Dictionary<string, Action<dynamic>>();

        public QueueCommandProcessor(params IQueueCommandModule[] modules)
        {
            foreach (var module in modules)
            {
                module.RegisterCommands(this);
            }
        }

        public void RegisterCommand(string command, Action<dynamic> func)
        {
            commands[command.ToLower()] = func;
        }

        public void Execute(string command, dynamic payload)
        {
            Action<dynamic> action;

            if (!commands.TryGetValue(command, out action))
            {
                logger.Warn("Received unknown command {0}", command);
                return;
            }

            action(payload);
        }
    }
}
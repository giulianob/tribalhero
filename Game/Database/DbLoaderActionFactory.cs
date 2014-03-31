using System;
using System.Collections.Generic;
using Game.Logic;
using Game.Logic.Actions;
using Game.Setup.DependencyInjection;

namespace Game.Database
{
    public class DbLoaderActionFactory
    {
        private readonly IKernel kernel;

        public DbLoaderActionFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public ScheduledPassiveAction CreateScheduledPassiveAction(Type type,
                                                                   uint id,
                                                                   DateTime beginTime,
                                                                   DateTime nextTime,
                                                                   DateTime endTime,
                                                                   bool isVisible,
                                                                   string nlsDescription,
                                                                   Dictionary<string, string> properties)
        {
            var action = (ScheduledPassiveAction) kernel.Get(type);
            action.LoadFromDatabase(id, beginTime, nextTime, endTime, isVisible, nlsDescription);
            action.LoadProperties(properties);

            return action;
        }

        public ScheduledActiveAction CreateScheduledActiveAction(Type type,
                                                                 uint id,
                                                                 DateTime beginTime,
                                                                 DateTime nextTime,
                                                                 DateTime endTime,
                                                                 int workerType,
                                                                 byte workerIndex,
                                                                 ushort actionCount,
                                                                 Dictionary<string, string> properties)
        {
            var action = (ScheduledActiveAction)kernel.Get(type);
            action.LoadFromDatabase(id, beginTime, nextTime, endTime, workerType, workerIndex, actionCount);
            action.LoadProperties(properties);

            return action;
        }

        public PassiveAction CreatePassiveAction(Type type,
                                                 uint id,
                                                 bool isVisible,
                                                 IDictionary<string, string> properties)
        {
            var action = (PassiveAction)kernel.Get(type);
            action.LoadFromDatabase(id, isVisible);
            action.LoadProperties(properties);

            return action;
        }

        public ChainAction CreateChainAction(Type type,
                                             uint id,
                                             string chainCallback,
                                             PassiveAction current,
                                             ActionState chainState,
                                             bool isVisible,
                                             IDictionary<string, string> properties)
        {
            var action = (ChainAction)kernel.Get(type);
            action.LoadFromDatabase(id, chainCallback, current, chainState, isVisible);
            action.LoadProperties(properties);

            return action;
        }
    }
}
using System;
using System.Collections.Generic;
using Game.Logic;
using Game.Logic.Actions;
using Ninject;
using Ninject.Parameters;

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
            return
                    (ScheduledPassiveAction)
                    kernel.Get(type,
                               new ConstructorArgument("id", id),
                               new ConstructorArgument("beginTime", beginTime),
                               new ConstructorArgument("nextTime", nextTime),
                               new ConstructorArgument("endTime", endTime),
                               new ConstructorArgument("isVisible", isVisible),
                               new ConstructorArgument("nlsDescription", nlsDescription),
                               new ConstructorArgument("properties", properties));
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
            return
                    (ScheduledActiveAction)
                    kernel.Get(type,
                               new ConstructorArgument("id", id),
                               new ConstructorArgument("beginTime", beginTime),
                               new ConstructorArgument("nextTime", nextTime),
                               new ConstructorArgument("endTime", endTime),
                               new ConstructorArgument("workerType", workerType),
                               new ConstructorArgument("workerIndex", workerIndex),
                               new ConstructorArgument("actionCount", actionCount),
                               new ConstructorArgument("properties", properties));
        }

        public PassiveAction CreatePassiveAction(Type type,
                                                 uint id,
                                                 bool isVisible,
                                                 IDictionary<string, string> properties)
        {
            return
                    (PassiveAction)
                    kernel.Get(type,
                               new ConstructorArgument("id", id),
                               new ConstructorArgument("isVisible", isVisible),
                               new ConstructorArgument("properties", properties));
        }

        public ChainAction CreateChainAction(Type type,
                                             uint id,
                                             string chainCallback,
                                             PassiveAction current,
                                             ActionState chainState,
                                             bool isVisible,
                                             IDictionary<string, string> properties)
        {
            return
                    (ChainAction)
                    kernel.Get(type,
                               new ConstructorArgument("id", id),
                               new ConstructorArgument("chainCallback", chainCallback),
                               new ConstructorArgument("current", current),
                               new ConstructorArgument("chainState", chainState),
                               new ConstructorArgument("isVisible", isVisible),
                               new ConstructorArgument("properties", properties));
        }
    }
}
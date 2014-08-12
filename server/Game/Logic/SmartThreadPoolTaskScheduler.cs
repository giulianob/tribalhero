using System;
using Amib.Threading;
using Common;
using Game.Util;
using Action = System.Action;

namespace Game.Logic
{
    public class SmartThreadPoolTaskScheduler : ITaskScheduler
    {
        private static readonly ILogger Logger = LoggerFactory.Current.GetLogger<Engine>();

        private readonly IWorkItemsGroup workItemsGroup;

        public SmartThreadPoolTaskScheduler(IWorkItemsGroup workItemsGroup)
        {
            this.workItemsGroup = workItemsGroup;
        }

        public void QueueWorkItem(Action task)
        {
            workItemsGroup.QueueWorkItem(delegate(WorkItemPriority priority)
            {
                try
                {
                    task();
                }
                catch(Exception e)
                {
                    Logger.Error("Unhandled exception in smartthreadpool queue work item");
                    Engine.UnhandledExceptionHandler(e);

                    throw;
                }
            }, WorkItemPriority.Normal);            

        }
    }
}
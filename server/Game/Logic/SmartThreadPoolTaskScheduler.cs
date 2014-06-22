using Amib.Threading;
using Action = System.Action;

namespace Game.Logic
{
    public class SmartThreadPoolTaskScheduler : ITaskScheduler
    {
        private readonly IWorkItemsGroup workItemsGroup;

        public SmartThreadPoolTaskScheduler(IWorkItemsGroup workItemsGroup)
        {
            this.workItemsGroup = workItemsGroup;
        }

        public void QueueWorkItem(Action task)
        {
            workItemsGroup.QueueWorkItem(() => task(), WorkItemPriority.Normal);
        }
    }
}
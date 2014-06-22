using System;

namespace Game.Logic
{
    public interface ITaskScheduler
    {
        void QueueWorkItem(Action task);
    }
}
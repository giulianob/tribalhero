using System;
using Game.Logic;
using Game.Map;
using System.Linq;
using Game.Util.Locking;
using Persistance;

namespace Game.Data.Stronghold
{
    class StrongholdActivationChecker : ISchedule
    {
        private readonly IStrongholdManager strongholdManager;
        private readonly IStrongholdActivationCondition strongholdActivationCondition;

        private TimeSpan timeSpan;

        public StrongholdActivationChecker(IStrongholdManager strongholdManager, IStrongholdActivationCondition strongholdActivationCondition, IWorld world, IDbManager dbManager)
        {
            this.strongholdManager = strongholdManager;
            this.strongholdActivationCondition = strongholdActivationCondition;
        }

        public void Start(TimeSpan timeSpan)
        {
            if (IsScheduled)
                return;
            this.timeSpan = timeSpan;
            Time = DateTime.UtcNow.Add(timeSpan);
            Scheduler.Current.Put(this);
        }

        public bool IsScheduled { get; set; }

        public DateTime Time { private set; get; }

        public void Callback(object custom)
        {
            foreach (IStronghold stronghold in strongholdManager.Where(s => s.StrongholdState == StrongholdState.Inactive).Where(stronghold => strongholdActivationCondition.ShouldActivate(stronghold)))
            {
                using (Concurrency.Current.Lock(stronghold))
                {
                    strongholdManager.Activate(stronghold);
                }
            }
            Time = DateTime.UtcNow.Add(timeSpan);
            Scheduler.Current.Put(this);
        }
    }
}

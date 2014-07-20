using System;
using System.Linq;
using Game.Logic;
using Game.Util.Locking;

namespace Game.Data.Stronghold
{
    public class StrongholdActivationChecker : ISchedule
    {
        private readonly ILocker locker;

        private readonly IScheduler scheduler;

        private readonly IStrongholdActivationCondition strongholdActivationCondition;

        private readonly IStrongholdManager strongholdManager;

        public StrongholdActivationChecker(IStrongholdManager strongholdManager,
                                           IStrongholdActivationCondition strongholdActivationCondition,
                                           IScheduler scheduler,
                                           ILocker locker)
        {
            this.strongholdManager = strongholdManager;
            this.strongholdActivationCondition = strongholdActivationCondition;
            this.scheduler = scheduler;
            this.locker = locker;
        }

        private TimeSpan TimeSpan { get; set; }

        public bool IsScheduled { get; set; }

        public DateTime Time { private set; get; }

        public void Callback(object custom)
        {
            foreach (IStronghold stronghold in
                    strongholdManager.Where(
                                            s =>
                                            s.StrongholdState == StrongholdState.Inactive &&
                                            strongholdActivationCondition.ShouldActivate(s)))
            {
                locker.Lock(stronghold)
                      .Do(() => strongholdManager.Activate(stronghold));
            }

            Time = DateTime.UtcNow.Add(TimeSpan);
            scheduler.Put(this);
        }

        public void Start(TimeSpan timeSpan)
        {
            if (IsScheduled)
            {
                return;
            }

            TimeSpan = timeSpan;
            Time = DateTime.UtcNow.Add(timeSpan);
            scheduler.Put(this);
        }
    }
}
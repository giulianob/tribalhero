﻿using System;
using Game.Logic;
using System.Linq;
using Game.Util.Locking;

namespace Game.Data.Stronghold
{
    class StrongholdActivationChecker : ISchedule
    {
        private readonly IStrongholdManager strongholdManager;

        private readonly IStrongholdActivationCondition strongholdActivationCondition;

        private readonly IScheduler scheduler;

        private readonly ILocker locker;

        private TimeSpan TimeSpan { get; set; }

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

        public void Start(TimeSpan timeSpan)
        {
            if (IsScheduled)
                return;

            TimeSpan = timeSpan;
            Time = DateTime.UtcNow.Add(timeSpan);
            scheduler.Put(this);
        }

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
                using (locker.Lock(stronghold))
                {
                    strongholdManager.Activate(stronghold);
                }
            }

            Time = DateTime.UtcNow.Add(TimeSpan);
            scheduler.Put(this);
        }
    }
}
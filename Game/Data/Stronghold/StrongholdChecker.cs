using System;
using System.Linq;
using Game.Logic;
using Game.Setup;
using Game.Util.Locking;
using Persistance;

namespace Game.Data.Stronghold
{
    public class StrongholdChecker : ISchedule
    {
        private readonly IDbManager dbManager;

        private readonly IScheduler scheduler;

        private readonly ILocker locker;

        private readonly IStrongholdManager strongholdManager;

        public StrongholdChecker(IStrongholdManager strongholdManager, IDbManager dbManager, IScheduler scheduler, ILocker locker)
        {
            this.strongholdManager = strongholdManager;
            this.dbManager = dbManager;
            this.scheduler = scheduler;
            this.locker = locker;
        }

        private void SetNextExecution()
        {
            if (Config.actions_instant_time)
            {
                Time = DateTime.UtcNow.AddSeconds(30);
            }
            else
            {
                Time = DateTime.UtcNow
                               .Subtract(new TimeSpan(0, 0, DateTime.UtcNow.Minute, DateTime.UtcNow.Second))
                               .AddHours(1);
            }
        }

        public void Start()
        {
            if (IsScheduled)
            {
                return;
            }

            SetNextExecution();
            scheduler.Put(this);
        }

        #region Implementation of ISchedule

        public bool IsScheduled { get; set; }

        public DateTime Time { get; private set; }

        public void Callback(object custom)
        {
            foreach (IStronghold stronghold in strongholdManager)
            {
                IMultiObjectLock multiObjecLock;
                if (stronghold.StrongholdState == StrongholdState.Occupied)
                {
                    multiObjecLock = locker.Lock(stronghold, stronghold.Tribe);
                    stronghold.Tribe.VictoryPoint += stronghold.VictoryPointRate;
                    dbManager.Save(stronghold.Tribe);
                }
                else
                {
                    multiObjecLock = locker.Lock(stronghold);
                }
                strongholdManager.UpdateGate(stronghold);
                dbManager.Save(stronghold);
                multiObjecLock.UnlockAll();
            }

            SetNextExecution();
            scheduler.Put(this);
        }

        #endregion
    }
}
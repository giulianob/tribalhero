using System;
using System.Linq;
using Game.Logic;
using Game.Setup;
using Game.Util.Locking;
using Persistance;

namespace Game.Data.Stronghold
{
    class VictoryPointChecker : ISchedule
    {
        private readonly IDbManager dbManager;

        private readonly IStrongholdManager strongholdManager;

        public VictoryPointChecker(IStrongholdManager strongholdManager, IDbManager dbManager)
        {
            this.strongholdManager = strongholdManager;
            this.dbManager = dbManager;
        }

        private void SetNextExecution()
        {
            if (Config.actions_instant_time)
            {
                Time = DateTime.UtcNow.AddSeconds(5);
            }
            else
            {
                Time =
                        DateTime.UtcNow.Subtract(new TimeSpan(0, 0, DateTime.UtcNow.Minute, DateTime.UtcNow.Second))
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
            Scheduler.Current.Put(this);
        }

        #region Implementation of ISchedule

        public bool IsScheduled { get; set; }

        public DateTime Time { get; private set; }

        public void Callback(object custom)
        {
            foreach (
                    IStronghold stronghold in
                            strongholdManager.Where(s => s.StrongholdState == StrongholdState.Occupied))
            {
                using (Concurrency.Current.Lock(stronghold, stronghold.Tribe))
                {
                    stronghold.Tribe.VictoryPoint += stronghold.VictoryPointRate;
                    dbManager.Save(stronghold.Tribe, stronghold);
                }
            }

            SetNextExecution();
            Scheduler.Current.Put(this);
        }

        #endregion
    }
}
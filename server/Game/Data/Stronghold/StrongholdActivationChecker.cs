using System;
using System.Linq;
using Game.Logic;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Persistance;

namespace Game.Data.Stronghold
{
    public class StrongholdActivationChecker : ISchedule
    {
        private readonly ILocker locker;

        private readonly ITileLocator tileLocator;

        private readonly ISystemVariableManager systemVariableManager;

        private readonly IScheduler scheduler;

        private readonly IStrongholdActivationCondition strongholdActivationCondition;

        private readonly IStrongholdManager strongholdManager;

        private readonly IDbManager dbManager;

        public StrongholdActivationChecker(IStrongholdManager strongholdManager,
                                           IStrongholdActivationCondition strongholdActivationCondition,
                                           IScheduler scheduler,
                                           ILocker locker,
                                           ITileLocator tileLocator,
                                           ISystemVariableManager systemVariableManager, 
                                           IDbManager dbManager)
        {
            this.strongholdManager = strongholdManager;
            this.strongholdActivationCondition = strongholdActivationCondition;
            this.scheduler = scheduler;
            this.locker = locker;
            this.tileLocator = tileLocator;
            this.systemVariableManager = systemVariableManager;
            this.dbManager = dbManager;
        }

        private TimeSpan TimeSpan { get; set; }

        public bool IsScheduled { get; set; }

        public DateTime Time { private set; get; }

        public void Callback(object custom)
        {
            foreach (IStronghold stronghold in strongholdManager.Where(s => s.StrongholdState == StrongholdState.Inactive && strongholdActivationCondition.ShouldActivate(s)))
            {
                locker.Lock(stronghold).Do(() => strongholdManager.Activate(stronghold));
            }

            // Activate stronghold with highest score if there are no neutral strongholds
            var lastNeutralActivationTimeVar = systemVariableManager["Stronghold.neutral_check"];
            var lastNeutralActivationTime = (DateTime)lastNeutralActivationTimeVar.Value;
            if (SystemClock.Now.Subtract(lastNeutralActivationTime).TotalHours >= 8)
            {                
                if (strongholdManager.All(s => s.StrongholdState != StrongholdState.Neutral))
                {
                    var mapCenter = new Position(Config.map_width / 2, Config.map_height / 2);
                    
                    var stronghold = strongholdManager.Where(s => s.StrongholdState == StrongholdState.Inactive && s.NearbyCitiesCount > 0)
                                                      .OrderByDescending(s => strongholdActivationCondition.Score(s))
                                                      .ThenBy(s => tileLocator.TileDistance(s.PrimaryPosition, 1, mapCenter, 1))
                                                      .FirstOrDefault();

                    // Do the same check for a SH but w/o nearby cities restriction
                    if (stronghold == null)
                    {
                        stronghold = strongholdManager.Where(s => s.StrongholdState == StrongholdState.Inactive)
                                                      .OrderByDescending(s => strongholdActivationCondition.Score(s))
                                                      .ThenBy(s => tileLocator.TileDistance(s.PrimaryPosition, 1, mapCenter, 1))
                                                      .FirstOrDefault();
                    }

                    if (stronghold != null)
                    {
                        locker.Lock(stronghold).Do(() => strongholdManager.Activate(stronghold));
                    }
                }
                
                using (dbManager.GetThreadTransaction())
                {
                    lastNeutralActivationTimeVar.Value = SystemClock.Now;
                    dbManager.Save(lastNeutralActivationTimeVar);
                }
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
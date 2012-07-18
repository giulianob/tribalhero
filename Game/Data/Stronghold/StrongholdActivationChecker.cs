using System;
using Game.Logic;
using Game.Map;
using System.Linq;

namespace Game.Data.Stronghold
{
    class StrongholdActivationChecker : ISchedule
    {
        private IStrongholdManager strongholdManager;
        private IStrongholdActivationCondition strongholdActivationCondition;
        private IWorld world;

        private TimeSpan timeSpan;

        public StrongholdActivationChecker(IStrongholdManager strongholdManager, IStrongholdActivationCondition strongholdActivationCondition, IWorld world)
        {
            this.strongholdManager = strongholdManager;
            this.strongholdActivationCondition = strongholdActivationCondition;
            this.world = world;
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
                stronghold.StrongholdState = StrongholdState.Neutral;
                ISimpleGameObject simpleGameObject = stronghold as ISimpleGameObject;
                if (simpleGameObject != null)
                {
                    simpleGameObject.BeginUpdate();
                    world.Add(simpleGameObject);
                    simpleGameObject.EndUpdate();
                }
            }
        }
    }
}

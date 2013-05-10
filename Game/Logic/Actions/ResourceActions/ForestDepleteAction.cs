#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Data.Forest;
using Game.Map;
using Game.Util;
using Game.Util.Locking;
using Ninject.Extensions.Logging;

#endregion

namespace Game.Logic.Actions.ResourceActions
{
    public class ForestDepleteAction : ISchedule
    {
        private readonly IForestManager forestManager;

        private readonly IRegionManager regionManager;

        private readonly ILocker locker;

        private readonly ILogger logger = LoggerFactory.Current.GetCurrentClassLogger();

        public ForestDepleteAction(IForest forest,
                                   DateTime time,
                                   IForestManager forestManager,
                                   IRegionManager regionManager,
                                   ILocker locker)
        {
            this.forestManager = forestManager;
            this.regionManager = regionManager;
            this.locker = locker;
            Forest = forest;
            Time = time;
        }

        public IForest Forest { get; private set; }

        #region ISchedule Members

        public DateTime Time { get; private set; }

        public bool IsScheduled { get; set; }

        public void Callback(object custom)
        {
            var lck = locker.Lock(forestManager.CallbackLockHandler, new object[] {Forest.ObjectId});
            using (lck)
            {
                logger.Debug(string.Format("Destroying forest[{0}]", Forest.ObjectId));

                var camps = new List<IStructure>(Forest);

                if (!Forest.InWorld)
                {
                    logger.Warn("Trying to remove forest that isnt even in the world");
                }

                foreach (var obj in camps)
                {
                    Forest.BeginUpdate();
                    Forest.RemoveLumberjack(obj);
                    Forest.EndUpdate();

                    obj.City.Owner.SendSystemMessage(null,
                                                     string.Format("{0}: Forest depleted", obj.City.Name),
                                                     string.Format("{0}: One of your lumbermill outposts have finished gathering wood from a forest. All laborers have returned to your city and are now idle.",
                                                                   obj.City.Name));

                    // Remove structure from city
                    obj.BeginUpdate();
                    regionManager.Remove(obj);
                    obj.City.ScheduleRemove(obj, false, true);
                    obj.EndUpdate();
                }

                forestManager.RemoveForest(Forest);
                lck.Dispose();
            }
        }

        #endregion
    }
}
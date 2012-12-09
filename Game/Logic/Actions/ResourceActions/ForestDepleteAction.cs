#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Map;
using Game.Util.Locking;

#endregion

namespace Game.Logic.Actions.ResourceActions
{
    public class ForestDepleteAction : ISchedule
    {
        public ForestDepleteAction(Forest forest, DateTime time)
        {
            Forest = forest;
            Time = time;
        }

        public Forest Forest { get; private set; }

        #region ISchedule Members

        public DateTime Time { get; private set; }

        public bool IsScheduled { get; set; }

        public void Callback(object custom)
        {
            var lck = Concurrency.Current.Lock(World.Current.Forests.CallbackLockHandler,
                                               new object[] {Forest.ObjectId},
                                               World.Current.Forests);
            using (lck)
            {
                Global.Logger.Debug(string.Format("Destroying forest[{0}]", Forest.ObjectId));

                var camps = new List<IStructure>(Forest);

                if (!Forest.InWorld)
                {
                    Global.Logger.Warn("Trying to remove forest that isnt even in the world");
                }

                foreach (var obj in camps)
                {
                    Forest.BeginUpdate();
                    Forest.RemoveLumberjack(obj);
                    Forest.EndUpdate();

                    obj.City.Owner.SendSystemMessage(null,
                                                     string.Format("{0}: Forest depleted", obj.City.Name),
                                                     string.Format(
                                                                   "{0}: One of your lumbermill outposts have finished gathering wood from a forest. All laborers have returned to your city and are now idle.",
                                                                   obj.City.Name));

                    // Remove structure from city
                    obj.BeginUpdate();
                    World.Current.Regions.Remove(obj);
                    obj.City.ScheduleRemove(obj, false, true);
                    obj.EndUpdate();
                }

                World.Current.Forests.RemoveForest(Forest);
                lck.Dispose();
            }
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using Game.Battle;
using Game.Logic;
using Game;

namespace Simulator
{
    public class BattleExecutor : BattleManager,ISchedule
    {
        TimeSpan delay = TimeSpan.FromSeconds(1);
        public TimeSpan Delay
        {
            get { return delay; }
            set { delay = value; }
        }
        DateTime time;
        public void execute()
        {
            Global.Scheduler.put(this);
           
        }

        public BattleExecutor(City owner):base(owner) {
            
        }

        #region ISchedule Members

        public DateTime Time
        {
            get { return time; }
        }

        public void callback(object custom)
        {
            if (this.executeTurn())
            {
                this.time = DateTime.Now.Add(delay);
                Global.Scheduler.put(this);
            }
        }

        #endregion
    }
}

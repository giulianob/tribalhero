using System;
using System.Linq;
using System.Text;
using Game.Data;
using Game.Logic;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Ninject;
using Persistance;

namespace Game.Module {
    public class PlayersRemover : ISchedule {
        double intervalInHours;
        bool started = false;
        ICityRemoverFactory iCityRemoverFactory;
        IPlayerSelector iPlayerSelector;

        public PlayersRemover(ICityRemoverFactory iCityRemoverFactory, IPlayerSelector iPlayerSelector)
        {
            this.iCityRemoverFactory = iCityRemoverFactory;
            this.iPlayerSelector = iPlayerSelector;
        }

        public void Start(double intervalInHours = 24) {
            this.intervalInHours = intervalInHours;
            started = true;
            Callback(null);
        }

        public void Stop()
        {
            started = false;
        }

        public int DeletePlayers() {
            var list = iPlayerSelector.GetPlayerIds();
            int count = 0;
            foreach (var id in list) {
                IPlayer player;
                using (Concurrency.Current.Lock(id, out player)) {
                    count += player.GetCityList().Count(city => iCityRemoverFactory.CreateCityRemover(city).Start());
                }
            }
            return count;
        }

        #region ISchedule Members

        public DateTime Time { get; private set; }
        public bool IsScheduled { get; set; }

        public void Callback(object custom) {
            if (!started) return;
            DeletePlayers();
            Time = DateTime.UtcNow.AddHours(intervalInHours);
            Scheduler.Current.Put(this);
        }

        #endregion
    }
}

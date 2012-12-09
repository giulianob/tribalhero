using System;
using System.Linq;
using Game.Data;
using Game.Logic;
using Game.Util.Locking;

namespace Game.Module
{
    public class PlayersRemover : ISchedule
    {
        private readonly ICityRemoverFactory cityRemoverFactory;

        private readonly IPlayerSelector playerSelector;

        private double intervalInHours;

        private bool started;

        public PlayersRemover(IPlayerSelector playerSelector, ICityRemoverFactory cityRemoverFactory)
        {
            this.cityRemoverFactory = cityRemoverFactory;
            this.playerSelector = playerSelector;
        }

        public void Start(double intervalInHours = 24)
        {
            this.intervalInHours = intervalInHours;
            started = true;
            Callback(null);
        }

        public void Stop()
        {
            started = false;
        }

        public int DeletePlayers()
        {
            var list = playerSelector.GetPlayerIds();
            int count = 0;
            foreach (var id in list)
            {
                IPlayer player;
                using (Concurrency.Current.Lock(id, out player))
                {
                    count += player.GetCityList().Count(city => cityRemoverFactory.CreateCityRemover(city.Id).Start());
                }
            }
            return count;
        }

        #region ISchedule Members

        public DateTime Time { get; private set; }

        public bool IsScheduled { get; set; }

        public void Callback(object custom)
        {
            if (!started)
            {
                return;
            }
            DeletePlayers();
            Time = DateTime.UtcNow.AddHours(intervalInHours);
            Scheduler.Current.Put(this);
        }

        #endregion
    }
}
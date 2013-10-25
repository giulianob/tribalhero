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

        private readonly IScheduler scheduler;

        private readonly ILocker locker;

        private readonly IPlayerSelector playerSelector;

        private double intervalInHours;

        public PlayersRemover(IPlayerSelector playerSelector, ICityRemoverFactory cityRemoverFactory, IScheduler scheduler, ILocker locker)
        {
            this.cityRemoverFactory = cityRemoverFactory;
            this.scheduler = scheduler;
            this.locker = locker;
            this.playerSelector = playerSelector;
        }

        public void Start(double intervalInHours = 24)
        {
            this.intervalInHours = intervalInHours;
            Time = DateTime.UtcNow.AddHours(intervalInHours);
            scheduler.Put(this);
        }
        
        public int DeletePlayers()
        {
            var list = playerSelector.GetPlayerIds();
            int count = 0;
            foreach (var id in list)
            {
                IPlayer player;
                using (locker.Lock(id, out player))
                {
                    count += player.GetCityList().Count(city => cityRemoverFactory.CreateCityRemover(city.Id).Start());
                }
            }
            return count;
        }

        public DateTime Time { get; private set; }

        public bool IsScheduled { get; set; }

        public void Callback(object custom)
        {
            DeletePlayers();
            Time = DateTime.UtcNow.AddHours(intervalInHours);
            scheduler.Put(this);
        }
    }
}
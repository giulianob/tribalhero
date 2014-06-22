using System;
using System.Linq;
using Common;
using Game.Comm;
using Game.Data;
using Game.Data.Store;
using Game.Logic;
using Game.Util;

namespace Game.Module
{
    public class StoreSync : ISchedule
    {
        private readonly ILogger logger = LoggerFactory.Current.GetLogger<StoreSync>();

        private readonly IScheduler scheduler;

        private readonly IThemeManager themeManager;

        private readonly IStoreManager storeManager;

        public bool IsScheduled { get; set; }

        public DateTime Time { get; set; }

        public StoreSync(IScheduler scheduler, IThemeManager themeManager, IStoreManager storeManager)
        {
            this.scheduler = scheduler;
            this.themeManager = themeManager;
            this.storeManager = storeManager;
        }

        public void Start()
        {
            Time = SystemClock.Now;
            scheduler.Put(this);
        }

        public void Callback(object custom)
        {
            var storeItems = ApiCaller.StoreItemGetAll();
            
            if (storeItems.Success)
            {
                themeManager.UpdateThemes(storeItems.Data.Select(storeItem => new Theme(storeItem.Id, storeItem.MinimumVersion)));

                storeManager.UpdateItems(storeItems.Data);
            }
            else
            {
                logger.Warn("Failed to list store items from main site {0}", storeItems.AllErrorMessages);
            }

            Time = SystemClock.Now.AddMinutes(30);
            scheduler.Put(this);
        }
    }
}
using System;
using Common;
using Game.Comm;
using Game.Data;
using Game.Logic;
using Game.Util;

namespace Game.Module
{
    public class StoreSync : ISchedule
    {
        private readonly ILogger logger = LoggerFactory.Current.GetLogger<StoreSync>();

        private readonly IScheduler scheduler;

        private readonly IThemeManager themeManager;

        public bool IsScheduled { get; set; }

        public DateTime Time { get; set; }

        public StoreSync(IScheduler scheduler, IThemeManager themeManager)
        {
            this.scheduler = scheduler;
            this.themeManager = themeManager;
        }

        public void Start()
        {
            Time = SystemClock.Now;
            scheduler.Put(this);
        }

        public void Callback(object custom)
        {
            var themesResponse = ApiCaller.ThemeGetAll();

            if (themesResponse.Success)
            {
                themeManager.UpdateThemes(themesResponse.Data);
            }
            else
            {
                logger.Warn("Failed to list themes from main site {0}", themesResponse.AllErrorMessages);
            }

            Time = SystemClock.Now.AddMinutes(30);
            scheduler.Put(this);
        }
    }
}
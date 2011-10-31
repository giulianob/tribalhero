using System;
using System.Collections.Generic;
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
    public class IdleChecker : ISchedule {
        public const double IDLE_HOURS = 14 * 24;
        public const double IDLE_DELETE_HOURS = 30 * 24;

        public void Start() {
            Callback(null);
        }

        #region ISchedule Members

        public DateTime Time { get; private set; }
        public bool IsScheduled { get; set; }

        public static void DeleteAllInactivePlayers() {
            using (var reader = Ioc.Kernel.Get<IDbManager>().ReaderQuery(
                                     string.Format(
                                                   "SELECT * FROM `{0}` WHERE TIMEDIFF(UTC_TIMESTAMP(), `last_login`) > '{1}:00:00.000000'",
                                                   Player.DB_TABLE, IDLE_DELETE_HOURS),
                                     new DbColumn[] { })) {
                while (reader.Read()) {
                    Player player;
                    using (Concurrency.Current.Lock((uint)reader["id"], out player)) {
                        foreach (City city in player.GetCityList()) {
                            CityRemover cr = new CityRemover(city.Id);
                            cr.Start();
                        }
                    }
                }
            }
        }

        public void Callback(object custom) {
            DeleteAllInactivePlayers();
            Time = DateTime.UtcNow.AddHours(IDLE_HOURS);
            Global.Scheduler.Put(this);
        }

        #endregion
    }
}

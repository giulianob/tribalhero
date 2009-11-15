using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Game.Database;

namespace Game.Logic {
    class SystemTimeUpdater {
        static Timer timer = new Timer(new TimerCallback(callback), null, Timeout.Infinite, Timeout.Infinite);

        public static void resume() {
            timer.Change(5000, 5000);
        }

        public static void pause() {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public static void callback(object obj) {
            using (DbTransaction transaction = Global.dbManager.GetThreadTransaction()) {
                Global.SystemVariables["System.time"].Value = DateTime.Now;
                Global.dbManager.Save(Global.SystemVariables["System.time"]);
            }
        }
    }
}

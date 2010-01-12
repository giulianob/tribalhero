#region

using System;
using System.Threading;
using Game.Data;
using Game.Database;

#endregion

namespace Game.Logic {
    class SystemTimeUpdater {
        private static Timer timer = new Timer(callback, null, Timeout.Infinite, Timeout.Infinite);

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
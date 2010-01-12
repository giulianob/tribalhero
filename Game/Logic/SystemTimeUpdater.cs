#region

using System;
using System.Threading;
using Game.Data;
using Game.Database;

#endregion

namespace Game.Logic {
    class SystemTimeUpdater {
        private static readonly Timer timer = new Timer(Callback, null, Timeout.Infinite, Timeout.Infinite);

        public static void Resume() {
            timer.Change(5000, 5000);
        }

        public static void Pause() {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public static void Callback(object obj) {
            using (DbTransaction transaction = Global.dbManager.GetThreadTransaction()) {
                Global.SystemVariables["System.time"].Value = DateTime.Now;
                Global.dbManager.Save(Global.SystemVariables["System.time"]);
            }
        }
    }
}
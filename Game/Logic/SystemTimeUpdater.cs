#region

using System;
using System.Threading;
using Game.Data;
using Game.Database;

#endregion

namespace Game.Logic {
    public class SystemTimeUpdater {
        private static readonly Timer Timer = new Timer(Callback, null, Timeout.Infinite, Timeout.Infinite);

        public static void Resume() {
            Timer.Change(1500, 1500);
        }

        public static void Pause() {
            Timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public static void Callback(object obj) {
            using (DbTransaction transaction = Global.DbManager.GetThreadTransaction()) {
                Global.SystemVariables["System.time"].Value = DateTime.UtcNow;
                Global.DbManager.Save(Global.SystemVariables["System.time"]);
            }
        }
    }
}
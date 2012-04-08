#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Game.Comm;
using Game.Data;
using Game.Database;
using Game.Map;
using Game.Setup;
using Game.Util;
using Ninject;
using Persistance;

#endregion

namespace Game.Logic
{
    public class SystemVariablesUpdater
    {
        private static Timer timer;
        private static readonly object objLock = new object();

        private static DateTime lastUpdateScheduler = DateTime.MinValue;

        private static DateTime systemStartTime = DateTime.MinValue;

        public static void Resume()
        {
            if (timer == null)
                timer = new Timer(Callback, null, Timeout.Infinite, Timeout.Infinite);

            timer.Change(1000, 1000);
        }

        public static void Pause()
        {
            if (timer != null)
                timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public static void Callback(object obj)
        {
            lock (objLock)
            {
                using (DbPersistance.Current.GetThreadTransaction())
                {                    
                    DateTime now = DateTime.UtcNow;

                    if (systemStartTime == DateTime.MinValue)
                        systemStartTime = now;

                    //System time
                    Global.SystemVariables["System.time"].Value = now;
                    DbPersistance.Current.Save(Global.SystemVariables["System.time"]);

                    if (DateTime.UtcNow.Subtract(lastUpdateScheduler).TotalMilliseconds < 5000)
                        return;

                    lastUpdateScheduler = now;

                    //Scheduler info
                    int schedulerSize;
                    int schedulerDelta;
                    int actionsFired;
                    DateTime lastProbe;
                    DateTime nextFire;

                    Scheduler.Current.Probe(out lastProbe, out actionsFired, out schedulerSize, out schedulerDelta, out nextFire);

                    int workerThreads;
                    int completionThreads;
                    int availableWorkerThreads;
                    int availableCompletionThreads;

                    ThreadPool.GetMaxThreads(out workerThreads, out completionThreads);
                    ThreadPool.GetAvailableThreads(out availableWorkerThreads, out availableCompletionThreads);

                    // Database info
                    int queriesRan;
                    DateTime lastDbProbe;
                    DbPersistance.Current.Probe(out queriesRan, out lastDbProbe);

                    var uptime = now.Subtract(systemStartTime);

                    var variables = new List<SystemVariable>
                                    {
                                            new SystemVariable("System.uptime", string.Format("{0} days {1:D2} hrs, {2:D2} mins, {3:D2} secs", (int)(uptime.TotalDays), uptime.Hours, uptime.Minutes, uptime.Seconds)),
                                            new SystemVariable("Scheduler.size", schedulerSize),
                                            new SystemVariable("Scheduler.size_change", schedulerDelta),
                                            new SystemVariable("Scheduler.actions_per_second", (int)(actionsFired/now.Subtract(lastProbe).TotalSeconds)),
                                            new SystemVariable("Scheduler.next_fire", nextFire),
                                            new SystemVariable("ThreadPool.max_worker", workerThreads),
                                            new SystemVariable("ThreadPool.max_completion", completionThreads),
                                            new SystemVariable("ThreadPool.available_worker", availableWorkerThreads),
                                            new SystemVariable("ThreadPool.available_completion", availableCompletionThreads),
                                            new SystemVariable("ThreadPool.active_worker", workerThreads - availableWorkerThreads),
                                            new SystemVariable("ThreadPool.active_completion", completionThreads - availableCompletionThreads),
                                            new SystemVariable("Process.memory_usage", Process.GetCurrentProcess().WorkingSet64),
                                            new SystemVariable("Process.peak_memory_usage", Process.GetCurrentProcess().PeakWorkingSet64),
                                            new SystemVariable("Database.queries_per_second", (int)(queriesRan/now.Subtract(lastDbProbe).TotalSeconds)),
                                            new SystemVariable("Players.count", World.Current.Players.Count),
                                            new SystemVariable("Players.logged_in", TcpWorker.GetSessionCount()),
                                            new SystemVariable("Cities.count", World.Current.CityCount),
                                            new SystemVariable("Channel.subscriptions", Global.Channel.SubscriptionCount()),
                                            new SystemVariable("Tribes.count", World.Current.Tribes.Count),
                                    };

                    // Max player logged in ever
                    using (DbDataReader reader = DbPersistance.Current.ReaderQuery(string.Format("SELECT * FROM `{0}` WHERE `name` = 'Players.max_logged_in' LIMIT 1", SystemVariable.DB_TABLE),
                                                 new DbColumn[] {}))
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            int maxLoggedIn = (int)DataTypeSerializer.Deserialize((string)reader["value"], (byte)reader["datatype"]);
                            int currentlyLoggedIn = TcpWorker.GetSessionCount();
                            if (currentlyLoggedIn > maxLoggedIn)
                            {
                                variables.AddRange(new List<SystemVariable>
                                                   {
                                                           new SystemVariable("Players.max_logged_in", currentlyLoggedIn),
                                                           new SystemVariable("Players.max_logged_in_date", DateTime.UtcNow),
                                                   });
                            }
                        }
                        else
                        {
                            variables.AddRange(new List<SystemVariable>
                                               {
                                                       new SystemVariable("Players.max_logged_in", 0),
                                                       new SystemVariable("Players.max_logged_in_date", DateTime.UtcNow),
                                               });
                        }
                    }

                    // Forest cnt
                    variables.AddRange(World.Current.Forests.ForestCount.Select((t, i) => new SystemVariable("Forests.lvl" + (i + 1) + "_count", t)));                    

                    // Update vars
                    foreach (var variable in variables)
                    {
                        if (!Global.SystemVariables.ContainsKey(variable.Key))
                            Global.SystemVariables.Add(variable.Key, variable);
                        else
                            Global.SystemVariables[variable.Key].Value = variable.Value;

                        DbPersistance.Current.Save(Global.SystemVariables[variable.Key]);
                    }
                }
            }
        }
    }
}
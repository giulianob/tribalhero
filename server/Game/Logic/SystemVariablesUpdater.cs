#region

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;
using Common;
using Dawn.Net.Sockets;
using Game.Comm;
using Game.Data;
using Game.Data.Forest;
using Game.Data.Stronghold;
using Game.Map;
using Game.Module;
using Game.Setup;
using Game.Util;
using Persistance;

#endregion

namespace Game.Logic
{
    public class SystemVariablesUpdater : IGameTask
    {
        private readonly ILogger logger = LoggerFactory.Current.GetLogger<SystemVariablesUpdater>();

        private readonly IDbManager dbManager;

        private readonly object objLock = new object();

        private readonly IScheduler scheduler;

        private readonly IStrongholdManager strongholdManager;

        private readonly IForestManager forestManager;

        private readonly ISystemVariableManager systemVariableManager;

        private readonly INetworkServer networkServer;

        private readonly IChannel channel;

        private readonly BlockingBufferManager bufferManager;

        private readonly SocketAwaitablePool socketAwaitablePool;

        private readonly ITribeManager tribeManager;

        private readonly IWorld world;

        private DateTime lastUpdateScheduler = DateTime.MinValue;

        private DateTime systemStartTime = DateTime.MinValue;

        private Timer timer;

        private readonly string graphiteKeyPrefix;

        public SystemVariablesUpdater(IWorld world,
                                      ITribeManager tribeManager,
                                      IDbManager dbManager,
                                      IScheduler scheduler,
                                      IStrongholdManager strongholdManager,
                                      IForestManager forestManager,
                                      ISystemVariableManager systemVariableManager,
                                      INetworkServer networkServer,
                                      IChannel channel,
                                      BlockingBufferManager bufferManager,
                                      SocketAwaitablePool socketAwaitablePool)
        {
            this.world = world;
            this.tribeManager = tribeManager;
            this.dbManager = dbManager;
            this.scheduler = scheduler;
            this.strongholdManager = strongholdManager;
            this.forestManager = forestManager;
            this.systemVariableManager = systemVariableManager;
            this.networkServer = networkServer;
            this.channel = channel;
            this.bufferManager = bufferManager;
            this.socketAwaitablePool = socketAwaitablePool;

            if (!string.IsNullOrEmpty(Config.api_id))
            {
                graphiteKeyPrefix = string.Format("servers.{0}_tribalhero_com.tribalhero.", Config.api_id);
            }
        }

        [Obsolete("Inject SystemVariablesUpdater instead")]
        public static SystemVariablesUpdater Current { get; set; }

        public void Resume()
        {
            if (timer == null)
            {
                timer = new Timer(Callback, null, Timeout.Infinite, Timeout.Infinite);
            }

            timer.Change(1000, 1000);
        }

        public void Pause()
        {
            if (timer != null)
            {
                timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        public void Callback(object obj)
        {
            lock (objLock)
            {
                using (dbManager.GetThreadTransaction())
                {
                    DateTime now = DateTime.UtcNow;

                    if (systemStartTime == DateTime.MinValue)
                    {
                        systemStartTime = now;
                    }

                    //System time
                    systemVariableManager["System.time"].Value = now;
                    dbManager.Save(systemVariableManager["System.time"]);

                    #region 10 second updates

                    if (DateTime.UtcNow.Subtract(lastUpdateScheduler).TotalMilliseconds < 10000)
                    {
                        return;
                    }

                    lastUpdateScheduler = now;

                    //Scheduler info
                    int schedulerSize;
                    int schedulerDelta;
                    int actionsFired;
                    DateTime lastProbe;
                    DateTime nextFire;
                    int actionTotalMilliseconds;

                    scheduler.Probe(out lastProbe, out actionsFired, out schedulerSize, out schedulerDelta, out nextFire, out actionTotalMilliseconds);

                    int workerThreads;
                    int completionThreads;
                    int availableWorkerThreads;
                    int availableCompletionThreads;

                    ThreadPool.GetMaxThreads(out workerThreads, out completionThreads);
                    ThreadPool.GetAvailableThreads(out availableWorkerThreads, out availableCompletionThreads);

                    // Database info
                    int queriesRan;
                    DateTime lastDbProbe;
                    dbManager.Probe(out queriesRan, out lastDbProbe);

                    // Stronghold info
                    int strongholdsNeutral;
                    int strongholdsOccupied;
                    strongholdManager.Probe(out strongholdsNeutral, out strongholdsOccupied);

                    var uptime = now.Subtract(systemStartTime);
                    var averageActionTime = actionsFired == 0 ? 0 : actionTotalMilliseconds / actionsFired;

                    var variables = new List<SystemVariable>
                    {
                        new SystemVariable("System.uptime", string.Format("{0} days {1:D2} hrs, {2:D2} mins, {3:D2} secs",
                                                                          (int)(uptime.TotalDays),
                                                                          uptime.Hours,
                                                                          uptime.Minutes,
                                                                          uptime.Seconds)),
                        new SystemVariable("Scheduler.size", schedulerSize),
                        new SystemVariable("Scheduler.average_action_time", averageActionTime),
                        new SystemVariable("Scheduler.size_change", schedulerDelta),
                        new SystemVariable("Scheduler.actions_per_second", (int)(actionsFired / now.Subtract(lastProbe).TotalSeconds)),
                        new SystemVariable("Scheduler.next_fire", nextFire),
                        new SystemVariable("ThreadPool.max_worker", workerThreads),
                        new SystemVariable("ThreadPool.max_completion", completionThreads),
                        new SystemVariable("ThreadPool.available_worker", availableWorkerThreads),
                        new SystemVariable("ThreadPool.available_completion", availableCompletionThreads),
                        new SystemVariable("ThreadPool.active_worker", workerThreads - availableWorkerThreads),
                        new SystemVariable("ThreadPool.active_completion", completionThreads - availableCompletionThreads),
                        new SystemVariable("Process.memory_usage", (int)(Process.GetCurrentProcess().WorkingSet64 / 1048576)),
                        new SystemVariable("Process.peak_memory_usage", (int)(Process.GetCurrentProcess().PeakWorkingSet64 / 1048576)),
                        new SystemVariable("Database.queries_per_second", (int)(queriesRan / now.Subtract(lastDbProbe).TotalSeconds)),
                        new SystemVariable("Players.count", world.Players.Count),
                        new SystemVariable("Players.logged_in", networkServer.GetSessionCount()),
                        new SystemVariable("Cities.count", world.Cities.Count),
                        new SystemVariable("Channel.subscriptions", channel.SubscriptionCount()),
                        new SystemVariable("Socket.buffer_manager_available", bufferManager.AvailableBuffers),
                        new SystemVariable("Socket.awaitable_pool_length", socketAwaitablePool.Count),
                        new SystemVariable("Tribes.count", tribeManager.TribeCount),
                        new SystemVariable("Strongholds.neutral", strongholdsNeutral),
                        new SystemVariable("Strongholds.occupied", strongholdsOccupied)
                    };

                    // Max player logged in ever
                    using (
                            DbDataReader reader =
                                    dbManager.ReaderQuery(
                                                          string.Format(
                                                                        "SELECT * FROM `{0}` WHERE `name` = 'Players.max_logged_in' LIMIT 1",
                                                                        SystemVariable.DB_TABLE),
                                                          new DbColumn[] {}))
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            int maxLoggedIn =
                                    (int)
                                    DataTypeSerializer.Deserialize((string)reader["value"], (byte)reader["datatype"]);
                            int currentlyLoggedIn = networkServer.GetSessionCount();
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
                    variables.Add(new SystemVariable("Forests.count", forestManager.ForestCount));

                    // Update vars                    
                    foreach (var variable in variables)
                    {
                        if (!systemVariableManager.ContainsKey(variable.Key))
                        {
                            systemVariableManager.Add(variable.Key, variable);
                        }
                        else
                        {
                            systemVariableManager[variable.Key].Value = variable.Value;
                        }

                        var actualVariable = systemVariableManager[variable.Key];
                        dbManager.Save(actualVariable);

                        if (!string.IsNullOrEmpty(graphiteKeyPrefix) && actualVariable.Value is int)
                        {
                            try
                            {
                                NStatsD.Client.Current.Gauge(string.Format("{0}{1}",
                                                                           graphiteKeyPrefix,
                                                                           actualVariable.Key.Replace('.', '-').ToLowerInvariant()),
                                                             (int)actualVariable.Value);
                            }
                            catch(Exception)
                            {
                            }
                        }
                    }

                    #endregion
                }
            }
        }
    }
}
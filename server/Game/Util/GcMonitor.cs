using System;
using System.Threading;
using Common;

namespace Game.Util
{
    public class GcMonitor
    {
        private static readonly ILogger Logger = LoggerFactory.Current.GetLogger<GcMonitor>();
        
        public static void MonitorGc()
        {
            GC.RegisterForFullGCNotification(10, 10);
            Thread startpolling = new Thread(() =>
            {
                while (true)
                {
                    // Check for a notification of an approaching collection.
                    GCNotificationStatus s = GC.WaitForFullGCApproach(1000);
                    if (s == GCNotificationStatus.Succeeded)
                    {
                        //Call event
                        Logger.Debug("GC is about to begin");
                        GC.Collect();

                    }
                    else if (s == GCNotificationStatus.Canceled)
                    {
                        // Cancelled the Registration
                    }
                    else if (s == GCNotificationStatus.Timeout)
                    {
                        // Timeout occurred.
                    }

                    // Check for a notification of a completed collection.
                    s = GC.WaitForFullGCComplete(1000);
                    if (s == GCNotificationStatus.Succeeded)
                    {
                        //Call event
                        Logger.Debug("GC has ended");
                    }
                    else if (s == GCNotificationStatus.Canceled)
                    {
                        //Cancelled the registration
                    }
                    else if (s == GCNotificationStatus.Timeout)
                    {
                        // Timeout occurred
                    }

                    Thread.Sleep(500);
                }
            });
            startpolling.Start();
        } 
    }
}
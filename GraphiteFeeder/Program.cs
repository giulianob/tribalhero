using System;
using System.IO;
using Ninject.Extensions.Logging.Log4net.Infrastructure;
using Persistance;
using Persistance.Managers;

namespace GraphiteFeeder
{
    class Program
    {
        private const string HOSTNAME = "127.0.0.1";
        private const string USERNAME = "root";
        private const string PASSWORD = "";
        private const string DATABASE = "tribalhero_server";
        private const int TIMEOUT = 9999999;
        private const int MAXCONNECTION = 50;
        private const bool VERBOSE = false;

        private static void Main(string[] args)
        {
            var dbManager = new MySqlDbManager(new Log4NetLoggerFactory().GetLogger(typeof(IDbManager)),
                                               HOSTNAME,
                                               USERNAME,
                                               PASSWORD,
                                               DATABASE,
                                               TIMEOUT,
                                               MAXCONNECTION,
                                               VERBOSE);

            // Obtain the file system entries in the directory path. 
            var dir = @"C:\source\gamemetric\";
            if (args.Length > 0)
                dir = args[0];

            foreach (var filename in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
            {
                IKpvConverter converter;
                var key = filename.Substring(dir.Length).TrimStart('\\', '/');

                if (filename.EndsWith(".list", StringComparison.InvariantCultureIgnoreCase))
                {
                    converter = new MultiValuesConverter(key.Remove(key.Length - 5), File.ReadAllText(filename), dbManager);
                }
                else if (filename.EndsWith(".value", StringComparison.InvariantCultureIgnoreCase))
                {
                    converter = new SingleValueConverter(key.Remove(key.Length - 6), File.ReadAllText(filename), dbManager);
                }
                else
                {
                    continue;
                }

                foreach (var kvp in converter)
                {
                    Console.WriteLine(kvp);
                    NStatsD.Client.Current.Gauge(kvp.Key, (int)kvp.Value);
                }
            }
        }
    }
}

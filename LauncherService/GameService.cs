#region

using System;
using System.ServiceProcess;
using System.Threading;
using CSVToXML;
using Game;
using Game.Setup;
using NDesk.Options;
using Ninject;
using log4net.Config;

#endregion

namespace LauncherService
{
    public partial class GameService : ServiceBase
    {
        private Engine engine;

        public GameService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            XmlConfigurator.Configure();

            var settingsFile = string.Empty;

            try
            {
                var p = new OptionSet {{"settings=", v => settingsFile = v},};
                p.Parse(Environment.GetCommandLineArgs());
            }
            catch(Exception)
            {
                Environment.Exit(0);
            }

            Config.LoadConfigFile(settingsFile);
            Factory.CompileConfigFiles();
            Engine.CreateDefaultKernel();
            Factory.InitAll();
            Converter.Go(Config.data_folder, Config.csv_compiled_folder, Config.csv_folder);

            engine = Ioc.Kernel.Get<Engine>();

            ThreadPool.QueueUserWorkItem(o =>
                {
                    if (!engine.Start())
                    {
                        throw new Exception("Failed to load server");
                    }
                });
        }

        protected override void OnStop()
        {
            if (engine != null)
            {
                engine.Stop();
            }
        }
    }
}
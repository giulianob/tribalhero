#region

using System;
using System.ServiceProcess;
using System.Threading;
using CSVToXML;
using Game;
using Game.Setup;
using NDesk.Options;
using Ninject;
using Ninject.Extensions.Logging.Log4net.Infrastructure;
using log4net;
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
                var p = new OptionSet {{"settings=", v => settingsFile = v}};
                p.Parse(Environment.GetCommandLineArgs());
            }
            catch(Exception)
            {
                Environment.Exit(0);
            }            

            ThreadPool.QueueUserWorkItem(o =>
                {
                    XmlConfigurator.Configure();
                    Engine.AttachExceptionHandler(new Log4NetLogger(typeof(Engine)));

                    Config.LoadConfigFile(settingsFile);
                    var kernel = Engine.CreateDefaultKernel();
                    kernel.Get<FactoriesInitializer>().CompileAndInit();
                    Converter.Go(Config.data_folder, Config.csv_compiled_folder, Config.csv_folder);

                    engine = kernel.Get<Engine>();

                    engine.Start();
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
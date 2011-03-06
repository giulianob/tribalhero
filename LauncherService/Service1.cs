#region

using System;
using System.ServiceProcess;
using CSVToXML;
using Game;
using Game.Setup;
using log4net;
using log4net.Config;

#endregion

namespace LauncherService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            XmlConfigurator.Configure();

            ILog logger = LogManager.GetLogger(typeof(Program));
            logger.Info("#######################################");

            Factory.CompileConfigFiles();            

            if (!Engine.Start())
                throw new Exception("Failed to load server");

            Converter.Go(Config.data_folder, Config.csv_compiled_folder, Config.csv_folder);
        }

        protected override void OnStop()
        {
            Engine.Stop();
        }
    }
}
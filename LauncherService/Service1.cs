using System;
using System.ServiceProcess;
using Game.Setup;
using log4net.Config;

namespace LauncherService {
    public partial class Service1 : ServiceBase {
        public Service1() {
            InitializeComponent();
        }
        
        protected override void OnStart(string[] args) {
            XmlConfigurator.Configure();
            Factory.CompileConfigFiles();
            CSVToXML.Converter.Go(Config.data_folder, Config.csv_compiled_folder, Config.csv_folder);

            if (!Game.Engine.Start()) {
                throw new Exception("Failed to load server");
            }
        }

        protected override void OnStop() {            
            Game.Engine.Stop();
        } 


    }
}

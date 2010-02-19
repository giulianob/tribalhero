using System;
using System.ServiceProcess;
using Game.Setup;

namespace LauncherService {
    public partial class Service1 : ServiceBase {
        public Service1() {
            InitializeComponent();
        }
        
        protected override void OnStart(string[] args) {
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

using System.ServiceProcess;

namespace LauncherService {
    public partial class Service1 : ServiceBase {
        public Service1() {
            InitializeComponent();
        }
        
        protected override void OnStart(string[] args) {
            Launcher.GameLauncher.Start();
        }

        protected override void OnStop() {
            Launcher.GameLauncher.Stop();
        } 


    }
}

#region

using System.ComponentModel;
using System.Configuration.Install;

#endregion

namespace LauncherService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }
    }
}
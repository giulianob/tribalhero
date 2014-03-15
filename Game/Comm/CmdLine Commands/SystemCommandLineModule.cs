using Game.Data;

namespace Game.Comm
{
    public class SystemCommandLineModule : CommandLineModule
    {
        private readonly INetworkServer networkServer;

        public SystemCommandLineModule(INetworkServer networkServer)
        {
            this.networkServer = networkServer;
        }

        public override void RegisterCommands(CommandLineProcessor processor)
        {
            processor.RegisterCommand("sessionstatus", SessionStatus, PlayerRights.Bureaucrat);            
            processor.RegisterCommand("disconnectall", DisconnectAll, PlayerRights.Bureaucrat);            
        }

        private string SessionStatus(Session session, string[] parms)
        {
            return networkServer.GetAllSessionStatus();
        }

        private string DisconnectAll(Session session, string[] parms)
        {
            return networkServer.DisconnectAll();
        }
    }
}
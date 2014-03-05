using Game.Data;

namespace Game.Comm
{
    public class SystemCommandLineModule : CommandLineModule
    {
        private readonly ITcpServer tcpServer;

        public SystemCommandLineModule(ITcpServer tcpServer)
        {
            this.tcpServer = tcpServer;
        }

        public override void RegisterCommands(CommandLineProcessor processor)
        {
            processor.RegisterCommand("socketstatus", SocketStatus, PlayerRights.Bureaucrat);            
            processor.RegisterCommand("disconnectall", DisconnectAll, PlayerRights.Bureaucrat);            
        }

        private string SocketStatus(Session session, string[] parms)
        {
            return tcpServer.GetAllSocketStatus();
        }

        private string DisconnectAll(Session session, string[] parms)
        {
            return tcpServer.DisconnectAll();
        }
    }
}
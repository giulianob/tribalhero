using Game.Data;

namespace Game.Comm
{
    public class SystemCommandLineModule : CommandLineModule
    {
        public override void RegisterCommands(CommandLineProcessor processor)
        {
            processor.RegisterCommand("socketstatus", SocketStatus, PlayerRights.Bureaucrat);            
            processor.RegisterCommand("disconnectall", DisconnectAll, PlayerRights.Bureaucrat);            
        }

        private string SocketStatus(Session session, string[] parms)
        {
            return TcpWorker.GetAllSocketStatus();
        }

        private string DisconnectAll(Session session, string[] parms)
        {
            return TcpWorker.DisconnectAll();
        }
    }
}
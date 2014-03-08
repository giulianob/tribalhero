namespace Game.Comm
{
    public interface INetworkServer
    {
        bool Start(string listenAddress, int port);

        bool Stop();

        int GetSessionCount();

        string GetAllSessionStatus();

        string DisconnectAll();
    }
}
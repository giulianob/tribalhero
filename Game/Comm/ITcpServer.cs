namespace Game.Comm
{
    public interface ITcpServer
    {
        bool Start();

        bool Stop();

        int GetSessionCount();

        string GetAllSocketStatus();

        string DisconnectAll();
    }
}
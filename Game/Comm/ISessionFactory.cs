using System.Net.Sockets;

namespace Game.Comm
{
    public interface ISocketSessionFactory
    {
        SocketSession CreateSocketSession(string name, Socket socket);
    }
}
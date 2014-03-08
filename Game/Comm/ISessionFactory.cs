using System.Net.Sockets;

namespace Game.Comm
{
    public interface ISocketSessionFactory
    {
        SynchronousSocketSession CreateSocketSession(string remoteIp, Socket socket);
        
        AsyncSocketSession CreateAsyncSocketSession(string remoteIp, Socket socket);
    }
}
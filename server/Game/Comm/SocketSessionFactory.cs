using System.Net.Sockets;
using Dawn.Net.Sockets;
using Game.Setup.DependencyInjection;

namespace Game.Comm
{
    public class SocketSessionFactory : ISocketSessionFactory
    {
        private readonly IKernel kernel;

        public SocketSessionFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public SynchronousSocketSession CreateSocketSession(string remoteIp, Socket socket)
        {
            return new SynchronousSocketSession(remoteIp, socket, kernel.Get<IProcessor>());
        }

        public AsyncSocketSession CreateAsyncSocketSession(string remoteIp, Socket socket)
        {
            return new AsyncSocketSession(remoteIp, socket, kernel.Get<IProcessor>(), kernel.Get<SocketAwaitablePool>(), kernel.Get<BlockingBufferManager>());
        }
    }
}
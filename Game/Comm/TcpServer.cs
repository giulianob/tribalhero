#region

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Game.Setup;

#endregion

namespace Game.Comm
{
    public class TcpServer
    {
        private readonly TcpListener listener;
        private readonly Thread listeningThread;
        private readonly int port = Config.server_port;
        private readonly Processor processor;
        private bool isStopped = true;

        public TcpServer()
        {
            IPAddress localAddr = IPAddress.Parse(Config.server_listen_address);
            if (localAddr == null)
                throw new Exception("Could not bind to listen address");

            listener = new TcpListener(localAddr, port);
            listeningThread = new Thread(ListenerHandler);
        }

        public TcpServer(Processor processor)
        {
            this.processor = processor;
            IPAddress localAddr = IPAddress.Parse(Config.server_listen_address);
            if (localAddr == null)
                throw new Exception("Could not bind to listen address");

            listener = new TcpListener(localAddr, port);
            listeningThread = new Thread(ListenerHandler);
        }

        public bool Start()
        {
            if (!isStopped)
                return false;
            isStopped = false;
            listeningThread.Start();
            return true;
        }

        public void ListenerHandler()
        {
            listener.Start();

            Socket s;
            while (!isStopped)
            {
                try
                {
                    s = listener.AcceptSocket();
                }
                catch(Exception)
                {
                    continue;
                }

                if (s.LocalEndPoint == null)
                    continue;

                s.Blocking = false;

                var session = new SocketSession(s.LocalEndPoint.ToString(), s, processor);

                TcpWorker.Add(session);                
            }

            listener.Stop();
        }

        public bool Stop()
        {
            if (isStopped)
                return false;

            isStopped = true;
            listener.Stop();
            listeningThread.Join();
            TcpWorker.DeleteAll();
            return true;
        }
    }
}
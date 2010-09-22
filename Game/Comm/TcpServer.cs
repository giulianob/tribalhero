#region

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Game.Data;
using Game.Setup;

#endregion

namespace Game.Comm {
    public class TcpServer {
        private readonly Thread listeningThread;

        private readonly TcpListener listener;
        private readonly int port = Config.server_port;
        private bool isStopped = true;
        private readonly Processor processor;

        public TcpServer() {
            IPAddress localAddr = IPAddress.Parse(Config.server_listen_address);
            if (localAddr == null)
                throw new Exception("Could not bind to listen address");

            listener = new TcpListener(localAddr, port);
            listeningThread = new Thread(ListenerHandler);
        }

        public TcpServer(Processor processor) {
            this.processor = processor;
            IPAddress localAddr = IPAddress.Parse(Config.server_listen_address);
            if (localAddr == null)
                throw new Exception("Could not bind to listen address");

            listener = new TcpListener(localAddr, port);
            listeningThread = new Thread(ListenerHandler);
        }

        public bool Start() {
            if (!isStopped)
                return false;
            isStopped = false;
            listeningThread.Start();
            return true;
        }

        public void ListenerHandler() {
            listener.Start();

           
            Socket s;            
            while (!isStopped) {
                try {
                    s = listener.AcceptSocket();
                }
                catch (Exception) {                    
                    continue;
                }

                if (s.LocalEndPoint == null)
                    continue;

                SocketSession session = new SocketSession(s.LocalEndPoint.ToString(), s, processor);

                ThreadPool.QueueUserWorkItem(TcpWorker.Add, session);
            }

            listener.Stop();
        }

        public bool Stop() {
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
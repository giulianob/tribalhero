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

            string policy = "<?xml version=\"1.0\"?>" +
                            "<!DOCTYPE cross-domain-policy SYSTEM \"/xml/dtds/cross-domain-policy.dtd\">" +
                            "<cross-domain-policy>" + "<site-control permitted-cross-domain-policies=\"all\"/>" +
                            "<allow-access-from domain=\"" + Config.flash_domain + "\" to-ports=\"" + Config.server_port +
                            "\" />" + "</cross-domain-policy>";

            Global.Logger.Info("Ready to serve policy file: " + policy);
            
            Socket s;            
            while (!isStopped) {
                try {
                    s = listener.AcceptSocket();
                }
                catch (Exception) {                    
                    continue;
                }
                byte[] buffer = new byte[128];

                int len;

                try {
                    len = s.Receive(buffer, SocketFlags.Peek);
                }
                catch (Exception) {
                    continue;
                }

                if (len == 23 && Encoding.ASCII.GetString(buffer, 0, len - 1) == "<policy-file-request/>")
                {                    
                    s.NoDelay = true;
                    s.LingerState = new LingerOption(true, 3);

                    byte[] xml = Encoding.UTF8.GetBytes(policy);
                    s.Send(xml);

                    Global.Logger.Info("Served policy file to " + s.RemoteEndPoint);

                    s.Close();

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
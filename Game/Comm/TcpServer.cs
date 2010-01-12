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
        private Thread listening_thread;

        private TcpListener listener;
        private readonly int port = Config.server_port;
        private bool isStopped = true;
        private Processor processor;

        public TcpServer() {
            IPAddress localAddr = IPAddress.Parse(Config.server_listen_address);
            listener = new TcpListener(localAddr, port);
            listening_thread = new Thread(new ThreadStart(listener_handler));
        }

        public TcpServer(Processor processor) {
            this.processor = processor;
            IPAddress localAddr = IPAddress.Parse(Config.server_listen_address);
            listener = new TcpListener(localAddr, port);
            listening_thread = new Thread(new ThreadStart(listener_handler));
        }

        public bool start() {
            if (!isStopped)
                return false;
            isStopped = false;
            listening_thread.Start();
            return true;
        }

        public void listener_handler() {
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
                    s = null;
                    s = listener.AcceptSocket();
                }
                catch (Exception e) {
                    Global.Logger.Error("Listener error", e);
                    return;
                }
                byte[] buffer = new byte[128];

                int len = 0;

                try {
                    len = s.Receive(buffer, SocketFlags.Peek);
                }
                catch (Exception) {
                    continue;
                }

                if (len == 23) {
                    s.NoDelay = true;
                    s.LingerState = new LingerOption(true, 3);

                    byte[] xml = Encoding.UTF8.GetBytes(policy);
                    s.Send(xml);

                    Global.Logger.Info("Served policy file to " + s.RemoteEndPoint.ToString());

                    s.Close();

                    continue;
                }

                SocketSession session = new SocketSession(s.LocalEndPoint.ToString(), s, processor);

                ThreadPool.QueueUserWorkItem(new WaitCallback(TcpWorker.add), session);
            }
            listener.Stop();
        }

        public bool stop() {
            if (isStopped)
                return false;
            TcpWorker.delAll();
            listener.Stop();
            listening_thread.Abort();
            isStopped = true;
            return true;
        }
    }
}
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
    public class PolicyServer {
        private readonly Thread listeningThread;

        private readonly TcpListener listener;
        private bool isStopped = true;

        public PolicyServer() {
            IPAddress localAddr = IPAddress.Parse(Config.server_listen_address);
            if (localAddr == null)
                throw new Exception("Could not bind to listen address");

            listener = new TcpListener(localAddr, 843);
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
                            "<cross-domain-policy>" + "<site-control permitted-cross-domain-policies=\"master-only\"/>" +
                            "<allow-access-from domain=\"" + Config.flash_domain + "\" to-ports=\"" + Config.server_port +
                            "\" />" + "</cross-domain-policy>";

            Global.Logger.Info("Ready to serve policy file: " + policy);
            
            Socket newSocket;
            while (!isStopped) {                
                try {
                    newSocket = listener.AcceptSocket();
                }
                catch (Exception) {                    
                    continue;
                }

                ThreadPool.QueueUserWorkItem(delegate(object state)
                {
                    Socket s = (Socket)state;

                    s.ReceiveTimeout = 1500;

                    byte[] buffer = new byte[128];

                    try
                    {
                        s.Receive(buffer, 23, SocketFlags.None);
                    }
                    catch (Exception)
                    {
                        if (s.Connected) s.Close();
                        return;
                    }

                    s.NoDelay = true;

                    byte[] xml = Encoding.UTF8.GetBytes(policy);
                    s.Send(xml);

                    Global.Logger.Info("Served policy file to " + s.RemoteEndPoint);

                    s.Close();
                }, newSocket);
            }

            listener.Stop();
        }

        public bool Stop() {
            if (isStopped)
                return false;
            
            isStopped = true;
            listener.Stop();
            listeningThread.Join();        
            return true;
        }
    }
}
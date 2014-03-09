#region

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Game.Setup;
using Game.Util;
using Ninject.Extensions.Logging;

#endregion

namespace Game.Comm
{
    public class PolicyServer : IPolicyServer
    {
        private readonly ILogger logger = LoggerFactory.Current.GetCurrentClassLogger();

        private TcpListener listener;

        private readonly Thread listeningThread;

        private readonly string policy;

        private readonly byte[] xmlBytes;

        private bool isStopped = true;

        public PolicyServer()
        {
            policy =
                    @"<?xml version=""1.0""?><!DOCTYPE cross-domain-policy SYSTEM ""/xml/dtds/cross-domain-policy.dtd""><cross-domain-policy><site-control permitted-cross-domain-policies=""master-only""/><allow-access-from domain=""" +
                    Config.flash_domain + @""" to-ports=""80,8085," + Config.server_port +
                    @""" /></cross-domain-policy>";

            xmlBytes = Encoding.UTF8.GetBytes(policy);

            listeningThread = new Thread(ListenerHandler);
        }

        public bool Start(string listenAddress, int port)
        {
            if (!isStopped)
            {
                return false;
            }

            IPAddress localAddr = IPAddress.Parse(listenAddress);
            listener = new TcpListener(localAddr, port);

            if (localAddr == null)
            {
                throw new Exception("Could not bind to listen address");
            }


            isStopped = false;
            listeningThread.Start();
            return true;
        }

        public bool Stop()
        {
            if (isStopped)
            {
                return false;
            }

            isStopped = true;
            listener.Stop();
            listeningThread.Join();
            return true;
        }

        private void ListenerHandler()
        {
            // Write policy to data folder
            File.WriteAllText(Path.Combine(Config.data_folder, "crossdomain.xml"), policy);

            logger.Info("Ready to serve policy file: " + policy);

            listener.Start();

            while (!isStopped)
            {
                Socket newSocket;
                try
                {
                    newSocket = listener.AcceptSocket();
                }
                catch(Exception)
                {
                    continue;
                }

                ThreadPool.UnsafeQueueUserWorkItem(delegate(object state)
                    {
                        var s = (Socket)state;
                        try
                        {
                            var buffer = new byte[128];

                            s.ReceiveTimeout = 3000;
                            s.Receive(buffer, 23, SocketFlags.None);
                            s.NoDelay = true;
                            s.Send(xmlBytes);

                            logger.Info("Served policy file to " + s.RemoteEndPoint);
                        }
                        catch
                        {
                        }

                        try
                        {
                            if (s.Connected)
                            {
                                s.Shutdown(SocketShutdown.Both);
                                s.Close();
                            }
                        }
                        catch
                        {
                        }
                    },
                                                   newSocket);
            }

            listener.Stop();
        }
    }
}
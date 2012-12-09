#region

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Game.Data;
using Game.Setup;

#endregion

namespace Game.Comm
{
    public interface IPolicyServer
    {
        bool Start();

        bool Stop();
    }

    public class PolicyServer : IPolicyServer
    {
        private readonly TcpListener listener;

        private readonly Thread listeningThread;

        private readonly string policy;

        private readonly byte[] xmlBytes;

        private bool isStopped = true;

        public PolicyServer()
        {
            IPAddress localAddr = IPAddress.Parse(Config.server_listen_address);
            if (localAddr == null)
            {
                throw new Exception("Could not bind to listen address");
            }

            policy =
                    @"<?xml version=""1.0""?><!DOCTYPE cross-domain-policy SYSTEM ""/xml/dtds/cross-domain-policy.dtd""><cross-domain-policy><site-control permitted-cross-domain-policies=""master-only""/><allow-access-from domain=""" +
                    Config.flash_domain + @""" to-ports=""80,8085," + Config.server_port +
                    @""" /></cross-domain-policy>";

            xmlBytes = Encoding.UTF8.GetBytes(policy);

            listener = new TcpListener(localAddr, 843);
            listeningThread = new Thread(ListenerHandler);
        }

        public bool Start()
        {
            if (!isStopped)
            {
                return false;
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

            Global.Logger.Info("Ready to serve policy file: " + policy);

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

                ThreadPool.QueueUserWorkItem(delegate(object state)
                    {
                        var s = (Socket)state;
                        try
                        {
                            var buffer = new byte[128];

                            s.ReceiveTimeout = 1500;
                            s.Receive(buffer, 23, SocketFlags.None);
                            s.NoDelay = true;
                            s.Send(xmlBytes);

                            Global.Logger.Info("Served policy file to " + s.RemoteEndPoint);
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
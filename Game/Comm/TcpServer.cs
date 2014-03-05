#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Game.Setup;

#endregion

namespace Game.Comm
{
    public class TcpServer : ITcpServer
    {        
        private readonly object workerLock = new object();

        private readonly List<TcpWorker> workerList = new List<TcpWorker>();

        private readonly TcpListener listener;

        private readonly Thread listeningThread;

        private readonly int port = Config.server_port;

        private readonly ISocketSessionFactory socketFactory;

        private bool isStopped = true;

        public TcpServer(ISocketSessionFactory socketFactory)
        {
            this.socketFactory = socketFactory;
            IPAddress localAddr = IPAddress.Parse(Config.server_listen_address);
            if (localAddr == null)
            {
                throw new Exception("Could not bind to listen address");
            }

            listener = new TcpListener(localAddr, port);
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
            DeleteAll();
            return true;
        }

        private void ListenerHandler()
        {
            listener.Start();

            Socket s;
            while (!isStopped)
            {
                SocketSession session;

                try
                {                
                    s = listener.AcceptSocket();

                    if (s.LocalEndPoint == null)
                    {
                        continue;
                    }

                    s.Blocking = false;
                    s.NoDelay = true;
                    s.SendTimeout = 1000;
                    s.SendBufferSize = 16384;

                    session = socketFactory.CreateSocketSession(s.RemoteEndPoint.ToString(), s);
                }
                catch(Exception)
                {
                    continue;
                }

                Add(session);
            }

            listener.Stop();
        }

        public int GetSessionCount()
        {
            lock (workerLock)
            {
                return workerList.Sum(x => x.Sessions.Count());
            }
        }

        public string GetAllSocketStatus()
        {
            var socketStatus = new StringBuilder();
            var total = 0;

            lock (workerLock)
            {
                foreach (var worker in workerList)
                {
                    lock (worker.SockListLock)
                    {
                        foreach (var session in worker.Sessions)
                        {
                            var socket = session.Socket;
                            try
                            {
                                socketStatus.AppendLine(string.Format("IP[{0}] Connected[{2}] Blocking[{1}]", session.Name, socket.Blocking, socket.Connected));
                            }
                            catch(Exception e)
                            {
                                socketStatus.AppendLine(string.Format("Failed to get socket status for {0}: {1}", session.Name, e.Message));
                            }

                            total++;
                        }
                    }
                }
            }

            socketStatus.Append(string.Format("{0} sockets total", total));

            return socketStatus.ToString();
        }

        public string DisconnectAll()
        {
            var socketStatus = new StringBuilder();
            lock (workerLock)
            {
                foreach (var worker in workerList)
                {
                    lock (worker.SockListLock)
                    {
                        foreach (var session in worker.Sessions.ToList())
                        {
                            worker.SocketDisconnect(session.Socket);
                        }
                    }
                }
            }

            return socketStatus.ToString();
        }

        private void Add(SocketSession session)
        {
            lock (workerLock)
            {
                bool needNewWorker = true;
                foreach (var worker in workerList)
                {
                    lock (worker.SockListLock)
                    {
                        // Worker full
                        if (worker.Sessions.Count > 250)
                        {
                            continue;
                        }

                        // Socket already disconnected before we got here
                        if (!session.Socket.Connected)
                        {
                            return;
                        }

                        session.OnClose += worker.OnClose;
                        worker.Put(session);
                    }

                    needNewWorker = false;
                    break;
                }

                if (needNewWorker)
                {
                    var newWorker = new TcpWorker();
                    workerList.Add(newWorker);

                    session.OnClose += newWorker.OnClose;

                    newWorker.Put(session);
                    newWorker.Start();
                }

                var packet = new Packet(Command.OnConnect);
                ThreadPool.QueueUserWorkItem(session.ProcessEvent, packet);
            }
        }

        private void DeleteAll()
        {
            lock (workerLock)
            {
                foreach (var worker in workerList)
                {
                    worker.Stop();
                }
            }
        }
    }
}
#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Game.Util;
using Ninject.Extensions.Logging;

#endregion

namespace Game.Comm
{
    internal class TcpWorker
    {
        private readonly ILogger logger = LoggerFactory.Current.GetCurrentClassLogger();

        private readonly Dictionary<Socket, SynchronousSocketSession> sessions = new Dictionary<Socket, SynchronousSocketSession>();

        private readonly ArrayList sockList = new ArrayList();

        private readonly object sockListLock = new object();

        private readonly EventWaitHandle socketAvailable = new EventWaitHandle(false, EventResetMode.AutoReset);

        private bool isStopped;

        private Thread workerThread;

        public object SockListLock
        {
            get
            {
                return sockListLock;
            }
        }

        public ICollection<SynchronousSocketSession> Sessions
        {
            get
            {
                return sessions.Values;
            }
        }

        public void Put(SynchronousSocketSession session)
        {
            // Already locked here btw from the Add call

            sessions.Add(session.Socket, session);

            sockList.Add(session.Socket);

            socketAvailable.Set();
        }

        public void Start()
        {
            if (workerThread == null)
            {
                workerThread = new Thread(SocketHandler) {Name = "TcpWorker Thread"};
            }

            if (workerThread.ThreadState != ThreadState.Running)
            {
                workerThread.Start();
            }
        }

        public void Stop()
        {
            isStopped = true;
            socketAvailable.Set();
            workerThread.Join();
        }

        public void OnClose(Session sender)
        {
            SocketDisconnect(((SynchronousSocketSession)sender).Socket);
        }

        public void SocketDisconnect(Socket s)
        {
            lock (SockListLock)
            {
                if (s.Connected)
                {
                    try
                    {
                        s.Shutdown(SocketShutdown.Both);
                    }
                    catch(SocketException) {}

                    s.Close(1);
                }

                //create disconnect packet to send to processor
                SynchronousSocketSession dcSession;
                if (!sessions.TryGetValue(s, out dcSession))
                {
                    // Socket already gone, probably happening because the socket handler saw it wasn't connected 
                    return;
                }

                if (dcSession.Player != null)
                {
                    dcSession.Player.HasTwoFactorAuthenticated = null;

                    logger.Info("Player disconnect {0}(1) IP: {2}", dcSession.Player.Name, dcSession.Player.PlayerId, dcSession.RemoteIP);
                }

                sessions.Remove(s);
                sockList.Remove(s);

                var packet = new Packet(Command.OnDisconnect);
                ThreadPool.QueueUserWorkItem(dcSession.ProcessEvent, packet);
            }
        }

        private void SocketHandler()
        {
            try
            {
                while (!isStopped)
                {
                    ArrayList copyList;

                    try
                    {
                        lock (SockListLock)
                        {
                            copyList = new ArrayList(sockList);
                        }

                        if (copyList.Count > 0)
                        {
                            Socket.Select(copyList, null, null, 3000);
                        }
                        else
                        {
                            socketAvailable.WaitOne(-1, false);
                        }
                    }
                    catch(Exception e)
                    {
                        logger.Info("Socket exception: " + e.Message);
                        continue;
                    }

                    foreach (Socket s in copyList)
                    {
                        lock (s)
                        {
                            try
                            {
                                if (!s.Connected)
                                {
                                    SocketDisconnect(s);
                                    continue;
                                }

                                var data = new byte[s.Available];

                                int len = s.Receive(data, 0, data.Length, SocketFlags.None);

                                if (len == 0)
                                {
                                    SocketDisconnect(s);
                                    continue;
                                }

                                var session = sessions[s];

                                session.PacketMaker.Append(data);

                                do
                                {
                                    Packet packet = session.PacketMaker.GetNextPacket();

                                    if (packet == null)
                                    {
                                        // Remove the player if after processing packets we still have over 1MB of data remaining.
                                        // This probably means the player is spamming the server with an invalid client that doesn't speak our protocol
                                        if (session.PacketMaker.Length > 1048576)
                                        {
                                            SocketDisconnect(s);
                                        }

                                        break;
                                    }

                                    ThreadPool.UnsafeQueueUserWorkItem(state => session.Process((Packet)state), packet);
                                }
                                while (true);
                            }
                            catch(SocketException)
                            {
                                SocketDisconnect(s);
                            }
                            catch(ObjectDisposedException)
                            {
                                // This exception happens if the client disconnects and we shut down the socket but then still try to read data
                            }
                        }
                    }
                }
            }
            catch(ThreadAbortException)
            {
            }
        }

        public void TryDelete(SynchronousSocketSession session)
        {
            if (sockList.Contains(session.Socket))
            {
                sockList.Remove(session.Socket);
            }
        }
    }
}
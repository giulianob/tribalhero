#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Game.Data;
using Game.Util;
using Ninject.Extensions.Logging;

#endregion

namespace Game.Comm
{
    class TcpWorker
    {
        private readonly ILogger logger = LoggerFactory.Current.GetCurrentClassLogger();

        private static readonly object workerLock = new object();

        private static readonly List<TcpWorker> workerList = new List<TcpWorker>();

        private readonly Dictionary<Socket, SocketSession> sessions = new Dictionary<Socket, SocketSession>();

        private readonly ArrayList sockList = new ArrayList();

        private readonly object sockListLock = new object();

        private readonly EventWaitHandle socketAvailable = new EventWaitHandle(false, EventResetMode.AutoReset);

        private bool isStopped;

        private Thread workerThread;

        public static int GetSessionCount()
        {
            lock (workerLock)
            {
                return workerList.Sum(x => x.sessions.Count);
            }
        }

        public static void Add(SocketSession session)
        {
            lock (workerLock)
            {
                bool needNewWorker = true;
                foreach (var worker in workerList)
                {
                    lock (worker.sockListLock)
                    {
                        // Worker full
                        if (worker.sessions.Count > 250)
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

        public static void DeleteAll()
        {
            lock (workerLock)
            {
                foreach (var worker in workerList)
                {
                    worker.Stop();
                }
            }
        }

        public static void Delete(SocketSession session)
        {
            lock (workerLock)
            {
                foreach (var worker in workerList)
                {
                    if (worker.sockList.Contains(session.Socket))
                    {
                        worker.sockList.Remove(session.Socket);
                    }
                }
            }
        }

        public void Put(SocketSession session)
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

        private void OnClose(Session sender)
        {
            SocketDisconnect(((SocketSession)sender).Socket);
        }

        private void SocketDisconnect(Socket s)
        {
            lock (sockListLock)
            {
                if (s.Connected)
                {
                    s.Shutdown(SocketShutdown.Both);
                    s.Close();
                }

                //create disconnect packet to send to processor
                SocketSession dcSession;
                if (!sessions.TryGetValue(s, out dcSession))
                {
                    // Socket already gone, probably happening because the socket handler saw it wasn't connected 
                    return;
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
                        lock (sockListLock)
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

                                    ThreadPool.UnsafeQueueUserWorkItem(session.Process, packet);
                                }
                                while (true);
                            }
                            catch(SocketException)
                            {
                                SocketDisconnect(s);
                            }
                        }
                    }
                }
            }
            catch(ThreadAbortException)
            {
            }
        }
    }
}
#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Game.Data;

#endregion

namespace Game.Comm
{
    class TcpWorker
    {
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
                        if (worker.sessions.Count > 250)
                            continue;

                        worker.Put(session);
                    }

                    needNewWorker = false;
                    break;
                }

                if (needNewWorker)
                {
                    var newWorker = new TcpWorker();
                    workerList.Add(newWorker);
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
                    worker.Stop();
            }
        }

        public static void Delete(SocketSession session)
        {
            lock (workerLock)
            {
                foreach (var worker in workerList)
                {
                    if (worker.sockList.Contains(session.Socket))
                        worker.sockList.Remove(session.Socket);
                }
            }
        }

        public void Put(SocketSession session)
        {
            lock (sockListLock)
            {
                sessions.Add(session.Socket, session);

                sockList.Add(session.Socket);

                socketAvailable.Set();
            }
        }

        public void Start()
        {
            if (workerThread == null)
                workerThread = new Thread(SocketHandler) {Name = "TcpWorker Thread"};

            if (workerThread.ThreadState != ThreadState.Running)
                workerThread.Start();
        }

        public void Stop()
        {
            isStopped = true;
            socketAvailable.Set();
            workerThread.Join();
        }

        private void SocketDisconnect(Socket s)
        {
            //create disconnect packet to send to processor
            SocketSession dcSession = sessions[s];

            sessions.Remove(s);
            sockList.Remove(s);

            var packet = new Packet(Command.OnDisconnect);
            ThreadPool.QueueUserWorkItem(dcSession.ProcessEvent, packet);
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
                            Socket.Select(copyList, null, null, 3000);
                        else
                            socketAvailable.WaitOne(-1, false);
                    }
                    catch(Exception e)
                    {
                        Global.Logger.Info("Socket exception: " + e.Message);
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

                                int len = s.Receive(data);

                                if (len == 0)
                                {
                                    SocketDisconnect(s);
                                    continue;
                                }

                                Global.Logger.Info("[" + sessions[s].Name + "]: " + data.Length);

                                sessions[s].AppendBytes(data);

                                Packet packet = sessions[s].GetNextPacket();

                                if (packet == null)
                                    continue;

                                ThreadPool.QueueUserWorkItem(sessions[s].Process, packet);
                            }
                            catch(SocketException)
                            {
                                lock (sockListLock)
                                {
                                    SocketDisconnect(s);
                                }
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
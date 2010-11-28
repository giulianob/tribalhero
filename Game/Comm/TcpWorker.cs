#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Linq;
using Game.Data;

#endregion

namespace Game.Comm {
    class TcpWorker {       
        private readonly ArrayList sockList = new ArrayList();
        private readonly object sockListLock = new object();
        private readonly Dictionary<Socket, SocketSession> sessions = new Dictionary<Socket, SocketSession>();
        private bool isStopped;        
        private Thread workerThread;

        private readonly EventWaitHandle socketAvailable = new EventWaitHandle(false, EventResetMode.AutoReset);

        private static readonly object WorkerLock = new object();
        private static readonly List<TcpWorker> WorkerList = new List<TcpWorker>();

        public static int GetSessionCount() {
            lock (WorkerLock) {
                return WorkerList.Sum(x => x.sessions.Count);
            }
        }

        public static void Add(object sessionObject) {
            lock (WorkerLock) {
                SocketSession session = (SocketSession) sessionObject;

                bool needNewWorker = true;
                foreach (TcpWorker worker in WorkerList) {
                    lock (worker.sockListLock) {
                        worker.Put(session);
                    }

                    needNewWorker = false;
                    break;
                }

                if (needNewWorker) {
                    TcpWorker newWorker = new TcpWorker();
                    WorkerList.Add(newWorker);
                    newWorker.Put(session);
                    newWorker.Start();
                }

                Packet packet = new Packet(Command.ON_CONNECT);
                ThreadPool.QueueUserWorkItem(session.ProcessEvent, packet);
            }
        }

        public static void DeleteAll() {
            lock (WorkerLock) {
                foreach (TcpWorker worker in WorkerList)
                    worker.Stop();
            }
        }

        public static void Delete(SocketSession session) {
            lock (WorkerLock) {
                foreach (TcpWorker worker in WorkerList) {
                    if (worker.sockList.Contains(session.socket))
                        worker.sockList.Remove(session.socket);
                }
            }
        }

        public void Put(SocketSession session) {
            lock (sockListLock) {
                sessions.Add(session.socket, session);

                sockList.Add(session.socket);

                socketAvailable.Set();
            }
        }

        public void Start() {
            if (workerThread == null) {
                workerThread = new Thread(SocketHandler) {
                                                             Name = "TcpWorker Thread"
                                                         };
            }

            if (workerThread.ThreadState != ThreadState.Running)
                workerThread.Start();
        }

        public void Stop() {
            isStopped = true;
            socketAvailable.Set();
            workerThread.Join();
        }

        private void SocketDisconnect(Socket s) {
            //create disconnect packet to send to processor
            SocketSession dcSession = sessions[s];

            sessions.Remove(s);
            sockList.Remove(s);

            var packet = new Packet(Command.ON_DISCONNECT);
            ThreadPool.QueueUserWorkItem(dcSession.ProcessEvent, packet);
        }

        private void SocketHandler() {
            try {
                while (!isStopped) {
                    ArrayList copyList;

                    try {
                        lock (sockListLock) {
                            copyList = new ArrayList(sockList);
                        }

                        if (copyList.Count > 0)
                            Socket.Select(copyList, null, null, 3000);
                        else
                            socketAvailable.WaitOne(-1, false);
                    }
                    catch (Exception e) {
                        Global.Logger.Info("Socket exception: " + e.Message);
                        continue;
                    }

                    foreach (Socket s in copyList) {
                        lock (s) {
                            Packet packet;
                            try {
                                if (!s.Connected) {
                                    SocketDisconnect(s);
                                    continue;
                                }

                                byte[] data = new byte[s.Available];

                                int len = s.Receive(data);

                                if (len == 0) {
                                    SocketDisconnect(s);
                                    continue;
                                }

                                Global.Logger.Info("[" + sessions[s].name + "]: " + data.Length);     
                               
                                sessions[s].AppendBytes(data);
                                while ((packet = (sessions[s]).GetNextPacket()) != null) {
                                    ThreadPool.QueueUserWorkItem(sessions[s].Process, packet);
                                }
                            }
                            catch (SocketException) {
                                lock (sockListLock) {
                                    SocketDisconnect(s);
                                }
                            }
                        }
                    }
                }
            }
            catch (ThreadAbortException) {}
        }
    }
}
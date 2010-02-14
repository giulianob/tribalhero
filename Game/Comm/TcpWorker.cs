#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Game.Data;

#endregion

namespace Game.Comm {
    class TcpWorker {
        private readonly ArrayList sockList = new ArrayList();
        private readonly object sockListLock = new object();
        private readonly Dictionary<Socket, SocketSession> sessions = new Dictionary<Socket, SocketSession>();
        private bool isFull = false;
        private Thread workerThread;
        private readonly EventWaitHandle socketAvailable = new EventWaitHandle(false, EventResetMode.AutoReset);

        private static readonly List<TcpWorker> WorkerList = new List<TcpWorker>();

        public static void Add(object sessionObject) {
            SocketSession session = (SocketSession) sessionObject;

            bool needNewWorker = true;
            foreach (TcpWorker worker in WorkerList) {
                lock (worker.sockListLock) {
                    if (worker.isFull)
                        continue;

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
            ThreadPool.QueueUserWorkItem(session.processEvent, packet);
        }

        public static void DeleteAll() {
            foreach (TcpWorker worker in WorkerList)
                worker.Stop();
        }

        public static void Delete(SocketSession session) {
            foreach (TcpWorker worker in WorkerList) {
                if (worker.sockList.Contains(session.socket))
                    worker.sockList.Remove(session.socket);
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
            workerThread.Abort();
        }

        private void SocketHandler() {
            try {
                while (true) {
                    ArrayList copyList;

                    try {
                        lock (sockListLock) {
                            copyList = new ArrayList(sockList);
                        }

                        if (copyList.Count > 0)
                            Socket.Select(copyList, null, null, 20);
                        else
                            socketAvailable.WaitOne(-1, false);
                    }
                    catch (Exception e) {
                        Console.Out.Write("Socket exception: " + e.Message);
                        continue;
                    }

                    foreach (Socket s in copyList) {
                        lock (s) {
                            Packet packet;
                            try {
                                if (!s.Connected) {
                                    //create disconnect packet to send to processor
                                    SocketSession dcSession = sessions[s];

                                    sessions.Remove(s);
                                    sockList.Remove(s);

                                    packet = new Packet(Command.ON_DISCONNECT);
                                    ThreadPool.QueueUserWorkItem(dcSession.processEvent, packet);
                                    continue;
                                }

                                byte[] data = new byte[s.Available];

                                int len = s.Receive(data);

                                if (len == 0) {
                                    //create disconnect packet to send to processor
                                    SocketSession dcSession = sessions[s];
                                    sessions.Remove(s);
                                    sockList.Remove(s);

                                    //If the player is null it means the player failed to authenticate or had some connection issue
                                    //in that case we don't want to create any events since he never really connected
                                    if (dcSession.Player != null) {
                                        dcSession.Player.Session = null;
                                        packet = new Packet(Command.ON_DISCONNECT);
                                        ThreadPool.QueueUserWorkItem(dcSession.processEvent, packet);
                                    }
                                } else {
                                    Global.Logger.Info("[" + sessions[s].name + "]: " + data.Length);
                                    //Global.Logger.Info(Convert.ToString(data));
                                    sessions[s].appendBytes(data);
                                    while ((packet = (sessions[s]).getNextPacket()) != null)
                                        ThreadPool.QueueUserWorkItem(sessions[s].process, packet);
                                }
                            }
                            catch (SocketException) {
                                lock (sockListLock) {
                                    //create disconnect packet to send to processor
                                    SocketSession dcSession = sessions[s];

                                    sessions.Remove(s);
                                    sockList.Remove(s);

                                    packet = new Packet(Command.ON_DISCONNECT);
                                    ThreadPool.QueueUserWorkItem(dcSession.processEvent, packet);
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
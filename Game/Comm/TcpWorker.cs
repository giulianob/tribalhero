using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Collections;
using System.Threading;
using System.Net.Sockets;

namespace Game.Comm {
    class TcpWorker {        
        ArrayList sockList = new ArrayList();
        object sockListLock = new object();
        Dictionary<Socket, SocketSession> sessions = new Dictionary<Socket, SocketSession>();
        bool isFull = false;
        Thread workerThread = null;
        EventWaitHandle socketAvailable = new EventWaitHandle(false, EventResetMode.AutoReset);

        private static List<TcpWorker> workerList = new List<TcpWorker>();

        public static void add(object session_object) {
            SocketSession session = (SocketSession)session_object;

            bool needNewWorker = true;
            foreach (TcpWorker worker in workerList) {
                if (!worker.isFull) {
                    worker.put(session);
                    needNewWorker = false;
                    break;
                }
            }

            if (needNewWorker) {
                TcpWorker newWorker = new TcpWorker();
                workerList.Add(newWorker);
                newWorker.put(session);
                newWorker.start();
            }

            Packet packet = new Packet(Command.ON_CONNECT);
            ThreadPool.QueueUserWorkItem(new WaitCallback(session.processEvent), packet);
        }

        public static void delAll() {
            foreach (TcpWorker worker in workerList) {
                worker.stop();
            }
        }

        public static void del(SocketSession session) {
            foreach (TcpWorker worker in workerList) {
                if (worker.sockList.Contains(session.socket)) {
                    worker.sockList.Remove(session.socket);
                }
            }
        }

        public void put(SocketSession session) {
            sessions.Add(session.socket, session);
            lock (sockListLock) {
                sockList.Add(session.socket);
            }

            socketAvailable.Set();
        }

        public void start() {
            if (workerThread == null) {
                workerThread = new Thread(new ThreadStart(socketHandler));
                workerThread.Name = "TcpWorker Thread";
            }

            if (workerThread.ThreadState != ThreadState.Running)
                workerThread.Start();
        }

        public void stop() {
            workerThread.Abort();
        }

        private void socketHandler() {
            Packet packet;
            while (true) {
                ArrayList copyList = null;

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
                        try {
                            if (!s.Connected) {
                                //create disconnect packet to send to processor
                                SocketSession dcSession = sessions[s];

                                sessions.Remove(s);
                                sockList.Remove(s);

                                packet = new Packet(Command.ON_DISCONNECT);
                                ThreadPool.QueueUserWorkItem(new WaitCallback(dcSession.processEvent), packet);
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
                                    ThreadPool.QueueUserWorkItem(new WaitCallback(dcSession.processEvent), packet);
                                }
                            } else {
                                Global.Logger.Info("[" + sessions[s].name + "]: " + data.Length);
                                Global.Logger.Info(Convert.ToString(data));
                                sessions[s].appendBytes(data);
                                while ((packet = ((Session)sessions[s]).getNextPacket()) != null) {
                                    ThreadPool.QueueUserWorkItem(new WaitCallback(sessions[s].process), packet);
                                }
                            }
                        }
                        catch (SocketException) {
                            lock (sockListLock) {
                                //create disconnect packet to send to processor
                                SocketSession dcSession = sessions[s];

                                sessions.Remove(s);
                                sockList.Remove(s);

                                packet = new Packet(Command.ON_DISCONNECT);
                                ThreadPool.QueueUserWorkItem(new WaitCallback(dcSession.processEvent), packet);
                            }
                        }
                    }
                }
            }
        }
    }
}

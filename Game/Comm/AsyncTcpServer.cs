using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Common;
using Dawn.Net.Sockets;
using Game.Setup;
using Game.Util;
using SocketAwaitablePool = Dawn.Net.Sockets.SocketAwaitablePool;

namespace Game.Comm
{
    public class AsyncTcpServer : INetworkServer
    {
        private readonly ILogger logger = LoggerFactory.Current.GetLogger<AsyncTcpServer>();

        private readonly ConcurrentDictionary<AsyncSocketSession, Task> sessions = new ConcurrentDictionary<AsyncSocketSession, Task>();

        private readonly ISocketSessionFactory socketSessionFactory;

        private readonly SocketAwaitablePool socketAwaitablePool;

        private readonly BlockingBufferManager bufferManager;

        private Socket listener;

        private bool isStopped = true;

        private Task listeningTask;

        private byte[] policyFile;

        public IPEndPoint LocalEndPoint
        {
            get
            {
                return listener != null ? (IPEndPoint)listener.LocalEndPoint : null;
            }
        }

        public AsyncTcpServer(ISocketSessionFactory socketSessionFactory, SocketAwaitablePool socketAwaitablePool, BlockingBufferManager bufferManager)
        {
            this.socketSessionFactory = socketSessionFactory;
            this.socketAwaitablePool = socketAwaitablePool;
            this.bufferManager = bufferManager;
        }

        public bool Start(string listenAddress, int port)
        {
            if (!isStopped)
            {
                return false;
            }

            isStopped = false;
            
            var policy =
                    @"<?xml version=""1.0""?><!DOCTYPE cross-domain-policy SYSTEM ""/xml/dtds/cross-domain-policy.dtd""><cross-domain-policy><site-control permitted-cross-domain-policies=""master-only""/><allow-access-from domain=""" +
                    Config.flash_domain + @""" to-ports=""80,8085," + Config.server_port +
                    @""" /></cross-domain-policy>";

            policyFile = Encoding.UTF8.GetBytes(policy);

            IPAddress localAddr = IPAddress.Parse(listenAddress);
            if (localAddr == null)
            {
                throw new Exception("Could not bind to listen address");
            }

            listener = new Socket(localAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(new IPEndPoint(localAddr, port));

            listeningTask = ListenerHandler().CatchUnhandledException();
            
            return true;
        }

        private async Task ListenerHandler()
        {
            listener.Listen(int.MaxValue);

            while (!isStopped)
            {
                var socketAwaitable = socketAwaitablePool.Take();
                socketAwaitable.Clear();

                var buffer = bufferManager.GetBuffer();
                socketAwaitable.Buffer = buffer;

                try
                {
                    var result = await listener.AcceptAsync(socketAwaitable);

                    if (result != SocketError.Success || socketAwaitable.AcceptSocket == null)
                    {
                        if (logger.IsDebugEnabled)
                        {
                            logger.Debug("Received error when accepting connection result[{0}]", result);
                        }

                        continue;
                    }

                    var socket = socketAwaitable.AcceptSocket;

                    logger.Info("Received connection from {0}", socket.RemoteEndPoint.ToString());

                    socket.NoDelay = true;
                    socket.SendTimeout = 1000;

                    var session = socketSessionFactory.CreateAsyncSocketSession(socket.RemoteEndPoint.ToString(), socket);

                    session.PacketMaker.Append(socketAwaitable.Transferred);

                    Add(session);
                }
                catch(SocketException e)
                {
                    logger.Info(e, "SocketException in ListenerHandler. Ignoring...");

                    continue;
                }
                catch(Exception e)
                {
                    logger.Warn(e, "Exception in ListenerHandler");

                    throw;
                }
                finally
                {
                    socketAwaitable.Clear();
                    socketAwaitablePool.Add(socketAwaitable);

                    bufferManager.ReleaseBuffer(buffer);
                }                
            }
        }

        public bool Stop()
        {
            if (isStopped)
            {
                return false;
            }

            logger.Info("Stopping async tcp server");

            isStopped = true;
            listener.Close();

            try
            {
                listeningTask.Wait();
            }
            // Incase task was already completed
            catch(Exception)
            {                
            }

            DisconnectAll();

            return true;
        }

        public int GetSessionCount()
        {
            return sessions.Count;
        }

        public string GetAllSessionStatus()
        {
            var socketStatus = new StringBuilder();
            var total = 0;

            foreach (var sessionAndTask in sessions)
            {
                var session = sessionAndTask.Key;                
                var socket = sessionAndTask.Key.Socket;

                try
                {
                    socketStatus.AppendLine(string.Format("IP[{0}] Connected[{2}] Blocking[{1}]", session.RemoteIP, socket.Blocking, socket.Connected));
                }
                catch(Exception e)
                {
                    socketStatus.AppendLine(string.Format("Failed to get socket status for {0}: {1}", session.RemoteIP, e.Message));
                }

                total++;
            }

            socketStatus.Append(string.Format("{0} sockets total", total));

            return socketStatus.ToString();
        }

        public string DisconnectAll()
        {
            foreach (var sessionAndTask in sessions.ToList())
            {
                sessionAndTask.Key.CloseSession();

                try
                {
                    sessionAndTask.Value.Wait();
                }
                catch(Exception)
                {               
                    // Incase task was already finished
                }
            }

            return string.Empty;
        }

        private void Add(AsyncSocketSession session)
        {
            session.OnClose += OnClose;

            var readTask = Task.Run(async () =>
            {
                await ReadLoop(session);
            }).CatchUnhandledException();

            sessions.TryAdd(session, readTask);
        }

        private async Task ReadLoop(AsyncSocketSession session)
        {
            session.ProcessEvent(new Packet(Command.OnConnect));

            var hasCheckedForPolicyFile = false;

            var socketAwaitable = socketAwaitablePool.Take();

            var buffer = bufferManager.GetBuffer();
            socketAwaitable.Buffer = buffer;

            try
            {
                do
                {
                    // Check if it's a policy file request
                    if (!hasCheckedForPolicyFile && session.PacketMaker.Length > 22)
                    {
                        hasCheckedForPolicyFile = true;

                        if (Encoding.UTF8.GetString(session.PacketMaker.GetBytes(), 0, 22) == "<policy-file-request/>")
                        {
                            logger.Debug("Serving policy file through game server to {0}", session.RemoteIP);
                            try
                            {
                                await session.SendAsyncImmediatelly(policyFile);
                                session.Socket.Shutdown(SocketShutdown.Both);
                                session.Socket.Close(1);
                            }
                            catch(Exception e)
                            {
                                if (logger.IsDebugEnabled)
                                {
                                    logger.Debug(e, "Handled exception while serving policy file on main game port");
                                }
                            }

                            return;
                        }
                    }

                    // Keep processing as many packets as we can
                    do
                    {
                        Packet packet = session.PacketMaker.GetNextPacket();

                        if (packet == null)
                        {
                            // Remove the player if after processing packets we still have over 1MB of data remaining.
                            // This probably means the player is spamming the server with an invalid client
                            if (session.PacketMaker.Length > 1048576)
                            {
                                DisconnectSession(session);
                                return;
                            }

                            break;
                        }

                        session.Process(packet);
                    }
                    while (true);

                    // Read more data
                    var result = await session.Socket.ReceiveAsync(socketAwaitable);
                    if (result != SocketError.Success || socketAwaitable.Transferred.Count == 0)
                    {
                        break;
                    }

                    session.PacketMaker.Append(socketAwaitable.Transferred);
                }
                while (true);

                DisconnectSession(session);
            }
            catch(SocketException e)
            {
                logger.Info(e, "Handled socket exception while receiving socket data IP[{0}]", session.RemoteIP);

                DisconnectSession(session);
            }
            catch(ObjectDisposedException)
            {
            }
            catch(Exception e)
            {
                logger.ErrorException("Unhandled exception in readloop", e);

                throw;
            }
            finally
            {
                socketAwaitable.Clear();
                socketAwaitablePool.Add(socketAwaitable);

                bufferManager.ReleaseBuffer(buffer);
            }
        }

        private void DisconnectSession(AsyncSocketSession session)
        {
            // Socket already gone, someone else already cleaned it up
            Task removedSocket;
            if (!sessions.TryRemove(session, out removedSocket))
            {
                return;
            }

            try
            {
                session.Socket.Shutdown(SocketShutdown.Both);
            }
            catch(Exception)
            {
            }

            session.Socket.Close();

            if (session.Player != null)
            {
                session.Player.HasTwoFactorAuthenticated = null;

                logger.Info("Player disconnect {0}(1) IP: {2}", session.Player.Name, session.Player.PlayerId, session.RemoteIP);
            }
            else
            {
                logger.Debug("Socket disconnect without logged in player IP[{0}]", session.RemoteIP);
            }

            Task.Run(() => session.ProcessEvent(new Packet(Command.OnDisconnect))).CatchUnhandledException();
        }

        private void OnClose(Session session)
        {
            DisconnectSession((AsyncSocketSession) session);
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Dawn.Net.Sockets;
using Game.Util;
using Ninject.Extensions.Logging;
using SocketAwaitablePool = Dawn.Net.Sockets.SocketAwaitablePool;

namespace Game.Comm
{
    public class AsyncTcpServer : ITcpServer
    {
        private readonly ILogger logger = LoggerFactory.Current.GetCurrentClassLogger();

        private readonly ConcurrentDictionary<AsyncSocketSession, Task> sessions = new ConcurrentDictionary<AsyncSocketSession, Task>();

        private readonly ISocketSessionFactory socketSessionFactory;

        private readonly SocketAwaitablePool socketAwaitablePool;

        private readonly BlockingBufferManager bufferManager;

        private Socket listener;

        private bool isStopped = true;

        private Task listeningTask;

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
            
            IPAddress localAddr = IPAddress.Parse(listenAddress);
            if (localAddr == null)
            {
                throw new Exception("Could not bind to listen address");
            }

            listener = new Socket(localAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(new IPEndPoint(localAddr, port));

            listeningTask = ListenerHandler();
            
            return true;
        }

        private async Task ListenerHandler()
        {
            listener.Listen(int.MaxValue);

            while (!isStopped)
            {
                AsyncSocketSession session;
                var socketAwaitable = socketAwaitablePool.Take();

                var buffer = bufferManager.GetBuffer();
                socketAwaitable.Buffer = buffer;

                try
                {
                    var result = await listener.AcceptAsync(socketAwaitable);

                    if (result != SocketError.Success || socketAwaitable.AcceptSocket == null)
                    {
                        continue;
                    }

                    var socket = socketAwaitable.AcceptSocket;

                    socket.NoDelay = true;
                    socket.SendTimeout = 1000;

                    session = socketSessionFactory.CreateAsyncSocketSession(socket.RemoteEndPoint.ToString(), socket);

                    session.PacketMaker.Append(socketAwaitable.Transferred);
                }
                catch(SocketException e)
                {
                    logger.Debug(e, "SocketException in ListenerHandler. Ignoring...");

                    continue;
                }
                catch(Exception e)
                {
                    logger.Debug(e, "Exception in ListenerHandler");

                    throw;
                }
                finally
                {
                    socketAwaitable.Clear();
                    socketAwaitablePool.Add(socketAwaitable);

                    bufferManager.ReleaseBuffer(buffer);
                }

                Add(session);
            }
        }

        public bool Stop()
        {
            if (isStopped)
            {
                return false;
            }

            isStopped = true;
            listener.Close();

            listeningTask.Wait();

            DisconnectAll();

            return true;
        }

        public int GetSessionCount()
        {
            return sessions.Count;
        }

        public string GetAllSocketStatus()
        {
            var socketStatus = new StringBuilder();
            var total = 0;

            foreach (var sessionAndTask in sessions)
            {
                var session = sessionAndTask.Key;                
                var socket = sessionAndTask.Key.Socket;

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

            socketStatus.Append(string.Format("{0} sockets total", total));

            return socketStatus.ToString();
        }

        public string DisconnectAll()
        {
            foreach (var sessionAndTask in sessions.ToList())
            {
                sessionAndTask.Key.CloseSession();
                sessionAndTask.Value.Wait();
            }

            return string.Empty;
        }

        private void Add(AsyncSocketSession session)
        {
            session.OnClose += OnClose;

            var readTask = Task.Run(async () =>
            {
                await ReadLoop(session);
            });

            sessions.TryAdd(session, readTask);
        }

        private async Task ReadLoop(AsyncSocketSession session)
        {
            session.ProcessEvent(new Packet(Command.OnConnect));

            var socketAwaitable = socketAwaitablePool.Take();

            var buffer = bufferManager.GetBuffer();
            socketAwaitable.Buffer = buffer;

            try
            {
                do
                {
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

                        // ReSharper disable once CSharpWarnings::CS4014
                        Task.Run(() => session.Process(packet));
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
            catch(SocketException)
            {
                DisconnectSession(session);
            }
            catch(ObjectDisposedException)
            {
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

                logger.Info("Player disconnect {0}(1) IP: {2}", session.Player.Name, session.Player.PlayerId, session.Name);
            }

            Task.Run(() => session.ProcessEvent(new Packet(Command.OnDisconnect)));
        }

        private void OnClose(Session session)
        {
            DisconnectSession((AsyncSocketSession) session);
        }
    }
}

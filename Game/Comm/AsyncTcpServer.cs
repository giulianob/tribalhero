using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Dawn.Net.Sockets;
using Game.Setup;
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

        public AsyncTcpServer(ISocketSessionFactory socketSessionFactory, SocketAwaitablePool socketAwaitablePool, BlockingBufferManager bufferManager)
        {
            this.socketSessionFactory = socketSessionFactory;
            this.socketAwaitablePool = socketAwaitablePool;
            this.bufferManager = bufferManager;
        }

        public bool Start()
        {
            if (!isStopped)
            {
                return false;
            }

            isStopped = false;
            
            IPAddress localAddr = IPAddress.Parse(Config.server_listen_address);
            if (localAddr == null)
            {
                throw new Exception("Could not bind to listen address");
            }

            listener = new Socket(localAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(new IPEndPoint(localAddr, Config.server_port));

            listeningTask = ListenerHandler();            
            
            return true;
        }

        private async Task ListenerHandler()
        {
            listener.Listen(int.MaxValue);

            // From docs: The amount of the buffer used internally varies based on the address family of the socket. The minimum buffer size required is 288 bytes.
            // If buffer is larger then we may lose data sent during login but 288 is minimum to establish a connection
            var buffer = new byte[288];            

            while (!isStopped)
            {
                AsyncSocketSession session;
                var socketAwaitable = socketAwaitablePool.Take();

                //socketAwaitable.Buffer = new ArraySegment<byte>(buffer, 0, buffer.Length);
                socketAwaitable.ShouldCaptureContext = true;
                
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
                }
                catch(Exception e)
                {
                    continue;
                }
                finally
                {
                    socketAwaitable.Clear();
                    socketAwaitablePool.Add(socketAwaitable);
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
            socketAwaitable.ShouldCaptureContext = true;

            var buffer = bufferManager.GetBuffer();
            socketAwaitable.Buffer = buffer;    

            try
            {
                do
                {
                    var result = await session.Socket.ReceiveAsync(socketAwaitable);
                    if (result != SocketError.Success || socketAwaitable.Transferred.Count == 0)
                    {
                        break;
                    }

                    session.PacketMaker.Append(socketAwaitable.Transferred);

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
                                HandleSessionDisconnect(session);
                            }

                            break;
                        }

                        Task.Run(() => session.Process(packet));
                    }
                    while (true);
                }
                while (true);

                HandleSessionDisconnect(session);
            }
            catch(SocketException)
            {
                HandleSessionDisconnect(session);                
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

        private void HandleSessionDisconnect(AsyncSocketSession session)
        {
            if (session.Socket.Connected)
            {
                try
                {
                    session.Socket.Shutdown(SocketShutdown.Both);
                }
                catch(SocketException)
                {
                }

                session.Socket.Close();
            }

            // Socket already gone, probably happening because the socket handler saw it wasn't connected 
            Task removedSocket;
            if (!sessions.TryRemove(session, out removedSocket))
            {
                return;
            }

            if (session.Player != null)
            {
                session.Player.HasTwoFactorAuthenticated = null;

                logger.Info("Player disconnect {0}(1) IP: {2}", session.Player.Name, session.Player.PlayerId, session.Name);
            }
            
            Task.Run(() => session.ProcessEvent(new Packet(Command.OnDisconnect)));
        }

        private void OnClose(Session session)
        {
            HandleSessionDisconnect((AsyncSocketSession) session);
        }
    }
}

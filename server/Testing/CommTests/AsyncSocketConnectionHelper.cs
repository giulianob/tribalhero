using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Dawn.Net.Sockets;
using Game.Comm;

namespace Testing.CommTests
{
    public class AsyncSocketConnectionHelper : IDisposable
    {
        private readonly TcpListener listener;

        public SocketAwaitablePool SocketAwaitablePool { get; private set; }

        public BlockingBufferManager BlockingBufferManager { get; private set; }

        private readonly Dictionary<AsyncSocketSession, Socket> serverSocketsBySession = new Dictionary<AsyncSocketSession, Socket>();

        public AsyncSocketConnectionHelper(int bufferSize = 1000, int bufferCount = 15)
        {
            listener = new TcpListener(IPAddress.Loopback, 0);
            SocketAwaitablePool = new SocketAwaitablePool(bufferCount);
            BlockingBufferManager = new BlockingBufferManager(bufferSize, bufferCount);

            listener.Start();
        }

        public int Port
        {
            get
            {
                return ((IPEndPoint)listener.LocalEndpoint).Port;
            }
        }

        public async Task<AsyncSocketSession> GetConnectedSocket(IProcessor processor)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var serverSocketAccept = listener.AcceptSocketAsync();

            socket.Connect(new IPEndPoint(IPAddress.Loopback, ((IPEndPoint)listener.LocalEndpoint).Port));
            await serverSocketAccept;

            var serverSocket = serverSocketAccept.Result;

            var session = new AsyncSocketSession(Guid.NewGuid().ToString(),
                                                 socket,
                                                 processor,
                                                 SocketAwaitablePool,
                                                 BlockingBufferManager);

            serverSocketsBySession.Add(session, serverSocket);

            return session;
        }

        public void CloseSessionFromServerSide(AsyncSocketSession session)
        {
            var serverSocket = serverSocketsBySession[session];
            serverSocket.Close();
        }

        public ArraySegment<byte> ReadDataSentFromSession(AsyncSocketSession session)
        {
            var receiveBuffer = new byte[16000];

            var serverSocket = serverSocketsBySession[session];

            int totalBytesRead = 0;
            while (serverSocket.Available > 0)
            {
                int bytesRead = serverSocket.Receive(receiveBuffer, totalBytesRead, 1000, SocketFlags.None);

                totalBytesRead += bytesRead;
                Thread.Sleep(10);
            }

            return new ArraySegment<byte>(receiveBuffer, 0, totalBytesRead);
        }

        public void Dispose()
        {
            listener.Stop();

            foreach (var sockets in serverSocketsBySession)
            {
                sockets.Key.Socket.Close();
                sockets.Value.Close();
            }
        }
    }
}
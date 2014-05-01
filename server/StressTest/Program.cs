using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Dawn.Net.Sockets;
using Game.Comm;
using Game.Setup;

namespace StressTest
{
    class Program
    {
        private static SocketAwaitablePool socketAwaitablePool;

        private static BlockingBufferManager blockingBufferManager;

        static void Main()
        {
            Run();
        }

        private static void Run()
        {
            socketAwaitablePool = new SocketAwaitablePool(50);
            blockingBufferManager = new BlockingBufferManager(1000, 50);

            while (true)
            {
                try
                {
                    var policySession = Connect();

                    policySession.SendAsyncImmediatelly(Encoding.UTF8.GetBytes("<policy-file-request/>\n")).Wait();

                    var newSession = Connect();

                    var loginPacket = new Packet(Command.Login);
                    loginPacket.AddInt16(0); // version
                    loginPacket.AddInt16(0); // revision
                    loginPacket.AddByte(0);
                    loginPacket.AddString("1234");
                    loginPacket.AddString("");
                    newSession.SendAsyncImmediatelly(loginPacket.GetBytes()).Wait();

                    policySession.Socket.Shutdown(SocketShutdown.Both);
                    policySession.Socket.Close();

                    newSession.Socket.Shutdown(SocketShutdown.Both);
                    newSession.Socket.Close();
                }
                catch (Exception) { }
            }
        }

        private static AsyncSocketSession Connect()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            socket.NoDelay = true;
            socket.SendTimeout = 1000;
            socket.LingerState = new LingerOption(true, 2);

            socket.Connect(new IPEndPoint(IPAddress.Loopback, Config.server_port));

            return new AsyncSocketSession(Guid.NewGuid().ToString(),
                                                 socket,
                                                 new Processor(),
                                                 socketAwaitablePool,
                                                 blockingBufferManager);

        }

        class Processor : IProcessor
        {
            public void RegisterCommand(Command cmd, Game.Comm.Processor.DoWork func)
            {                
            }

            public void RegisterEvent(Command cmd, Game.Comm.Processor.DoWork func)
            {
            }

            public void Execute(Session session, Packet packet)
            {
            }

            public void ExecuteEvent(Session session, Packet packet)
            {
            }
        }
    }
}

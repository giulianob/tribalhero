using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Common.Testing;
using Dawn.Net.Sockets;
using FluentAssertions;
using Game.Comm;
using Game.Data;
using NSubstitute;
using Xunit.Extensions;

namespace Testing.CommTests
{
    public class AsyncTcpServerTests
    {
        [Theory, AutoNSubstituteData]
        public async Task ServerIsAbleToProcessPacketsAndCleansUpResources(ISocketSessionFactory sessionFactory, IProcessor processor, IPlayer player)
        {
            player.GetCityCount().Returns(1);
            player.HasTwoFactorAuthenticated = DateTime.Now;

            var socketAwaitablePool = new SocketAwaitablePool(10);
            var buffer = new BlockingBufferManager(1000, 15);
            
            Socket serverSideSocket = null;
            AsyncSocketSession session = null;

            sessionFactory.CreateAsyncSocketSession(Arg.Any<string>(), Arg.Any<Socket>())
                          .Returns(args =>
                          {
                              serverSideSocket = args.Arg<Socket>();
                              session = new AsyncSocketSession(string.Empty, serverSideSocket, processor, socketAwaitablePool, buffer)
                              {
                                  Player = player
                              };
                              return session;
                          });
            
            AsyncTcpServer server = new AsyncTcpServer(sessionFactory, socketAwaitablePool, buffer);
            try
            {
                server.Start("127.0.0.1", 0);

                // Connect and verify client connected and socket options are set
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    NoDelay = true
                };

                var packetToSend1 = new Packet(Command.MarketSell);
                packetToSend1.AddString("Hello");                
                var bytesToSendOnConnect = packetToSend1.GetBytes();
                await socket.ConnectAsync(new SocketAwaitable
                {
                    RemoteEndPoint = new IPEndPoint(IPAddress.Loopback, server.LocalEndPoint.Port),
                    Buffer = new ArraySegment<byte>(bytesToSendOnConnect, 0, bytesToSendOnConnect.Length)
                });

                // Give time for the connection to happen
                await Task.Delay(5000);

                server.GetSessionCount().Should().Be(1);
                serverSideSocket.Connected.Should().BeTrue();
                serverSideSocket.NoDelay.Should().BeTrue();
                serverSideSocket.SendTimeout.Should().Be(1000);

                // Send packets and verify server processes packets
                var packetToSend2 = new Packet(Command.MarketBuy);
                packetToSend2.AddString("World");                
                var bytesToSecondAfterConnect = packetToSend2.GetBytes();
                await socket.SendAsync(new SocketAwaitable
                {
                    Buffer = new ArraySegment<byte>(bytesToSecondAfterConnect, 0, bytesToSecondAfterConnect.Length)
                });

                await Task.Delay(5000);
                
                // Stop server and verify it disconnect and releases resources
                server.Stop();

                await Task.Delay(1000);

                player.HasTwoFactorAuthenticated.Should().Be(null);

                serverSideSocket.Connected.Should().BeFalse();
                socketAwaitablePool.Count.Should().Be(10);
                buffer.AvailableBuffers.Should().Be(15);

                processor.Received(1).Execute(session, Arg.Is<Packet>(packet => packet.GetBytes().SequenceEqual(bytesToSendOnConnect)));
                processor.Received(1).Execute(session, Arg.Is<Packet>(packet => packet.GetBytes().SequenceEqual(bytesToSecondAfterConnect)));
                processor.Received(1).ExecuteEvent(session, Arg.Is<Packet>(packet => packet.Cmd == Command.OnDisconnect));
            }
            finally
            {
                server.Stop();
            }
        }
    }
}
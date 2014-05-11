using System.Threading;
using System.Threading.Tasks;
using Common.Testing;
using FluentAssertions;
using Game.Comm;
using Xunit.Extensions;

namespace Testing.CommTests
{
    public class AsyncSocketSessionTests
    {
        [Theory, AutoNSubstituteData]
        public async Task Write_WhenSinglePacket_ShouldSendPacketAndCleanUpBuffers(IProcessor processor)
        {
            using (var connectionHelper = new AsyncSocketConnectionHelper(bufferCount: 10))
            {
                var session = await connectionHelper.GetConnectedSocket(processor);

                var packet = new Packet(Command.Login);
                packet.AddString("Hello");
                session.Write(packet);

                var receivedData = connectionHelper.ReadDataSentFromSession(session);

                var receivedPacket = new Packet(receivedData.Array, 0, receivedData.Count);
                receivedPacket.GetString().Should().Be("Hello");
                connectionHelper.BlockingBufferManager.AvailableBuffers.Should().Be(10);
                connectionHelper.SocketAwaitablePool.Count.Should().Be(10);
            }          
        }

        [Theory, AutoNSubstituteData]
        public async Task Write_WhenPacketIsLargerThanBuffer_ShouldSendPacketInMultiplePieces(IProcessor processor)
        {
            using (var connectionHelper = new AsyncSocketConnectionHelper(bufferSize: 500, bufferCount: 10))
            {
                var session = await connectionHelper.GetConnectedSocket(processor);

                var dataToSend = new string('*', 700);
                var packet = new Packet(Command.Login);
                packet.AddString(dataToSend);
                session.Write(packet);

                var receivedData = connectionHelper.ReadDataSentFromSession(session);

                var receivedPacket = new Packet(receivedData.Array, 0, receivedData.Count);
                receivedPacket.GetString().Should().Be(dataToSend);
            }          
        }

        [Theory, AutoNSubstituteData]
        public async Task Write_WhenMultipleLargePackets_ShouldSendInOrder(IProcessor processor)
        {
            using (var connectionHelper = new AsyncSocketConnectionHelper(bufferSize: 500, bufferCount: 10))
            {
                var session = await connectionHelper.GetConnectedSocket(processor);

                var dataToSend1 = new string('*', 700);
                var dataToSend2 = new string('!', 100);

                var packet1 = new Packet(Command.Login);
                packet1.AddString(dataToSend1);
                session.Write(packet1);

                var packet2 = new Packet(Command.Login);
                packet2.AddString(dataToSend2);
                session.Write(packet2);

                var receivedData = connectionHelper.ReadDataSentFromSession(session);
                session.PacketMaker.Append(receivedData);

                var receivedPacket1 = session.PacketMaker.GetNextPacket();
                receivedPacket1.GetString().Should().Be(dataToSend1);
                var receivedPacket2 = session.PacketMaker.GetNextPacket();
                receivedPacket2.GetString().Should().Be(dataToSend2);
                
                // No data should be left over
                session.PacketMaker.Length.Should().Be(0);
            }          
        }

        [Theory, AutoNSubstituteData]
        public async Task Write_WhenSocketIsDisconnected_ShouldContinue(IProcessor processor)
        {
            using (var connectionHelper = new AsyncSocketConnectionHelper(bufferSize: 500, bufferCount: 10))
            {
                var session = await connectionHelper.GetConnectedSocket(processor);

                var dataToSend1 = new string('*', 700);
                
                var packet1 = new Packet(Command.Login);
                packet1.AddString(dataToSend1);
                session.Write(packet1);

                connectionHelper.CloseSessionFromServerSide(session);

                session.Write(packet1);

                Thread.Sleep(1000);

                connectionHelper.BlockingBufferManager.AvailableBuffers.Should().Be(10);
                connectionHelper.SocketAwaitablePool.Count.Should().Be(10);
            }          
        }        
    }
}

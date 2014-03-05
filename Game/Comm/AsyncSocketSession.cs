#region

using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Dawn.Net.Sockets;

#endregion

namespace Game.Comm
{
    public class AsyncSocketSession : Session
    {
        private readonly SocketAwaitablePool socketAwaitablePool;

        private readonly BlockingBufferManager bufferManager;

        private readonly ConcurrentQueue<Packet> sendQueue = new ConcurrentQueue<Packet>();

        private int writerCount;

        public AsyncSocketSession(string name, Socket socket, Processor processor, SocketAwaitablePool socketAwaitablePool, BlockingBufferManager bufferManager)
                : base(name, processor)
        {
            this.socketAwaitablePool = socketAwaitablePool;
            this.bufferManager = bufferManager;
            Socket = socket;
        }

        public Socket Socket { get; private set; }

        public override void Write(Packet packet)
        {
            sendQueue.Enqueue(packet);
            if (Interlocked.CompareExchange(ref writerCount, 1, 0) == 0)
            {
                // **We** are the writer
                // Concept from: http://stackoverflow.com/questions/11648611/sending-data-in-order-with-socketasynceventargs
                ProcessQueue();
            }
        }

        private async Task ProcessQueue()
        {
            try
            {
                while (!sendQueue.IsEmpty)
                {
                    Packet packet;
                    if (sendQueue.TryDequeue(out packet))
                    {
                        await SendAsync(packet);
                    }
                }
            }
            finally
            {
                Interlocked.Exchange(ref writerCount, 0);
            }
        }

        private async Task SendAsync(Packet packet)
        {
            var packetBytes = packet.GetBytes();
            int totalBytesSent = 0;
            
            var socketAwaitable = socketAwaitablePool.Take();

            var sendBuffer = bufferManager.GetBuffer();
            var sendBufferMaxSize = sendBuffer.Count;

            try
            {
                while (totalBytesSent < packetBytes.Length)
                {
                    socketAwaitable.Clear();
                    var writeCount = Math.Min(packetBytes.Length - totalBytesSent, sendBufferMaxSize);
                    Buffer.BlockCopy(packetBytes, totalBytesSent, sendBuffer.Array, sendBuffer.Offset, writeCount);

                    socketAwaitable.Buffer = new ArraySegment<byte>(sendBuffer.Array, sendBuffer.Offset, writeCount);
                    
                    var result = await Socket.SendAsync(socketAwaitable);

                    if (result != SocketError.Success || socketAwaitable.Transferred.Count == 0)
                    {
                        return;
                    }

                    totalBytesSent += socketAwaitable.Transferred.Count;
                }
            }
            // Ignore cases where we accidentally send to the socket after its been disposed
            catch (ObjectDisposedException) { }
            finally
            {
                socketAwaitable.Clear();
                socketAwaitablePool.Add(socketAwaitable);

                bufferManager.ReleaseBuffer(sendBuffer);
            }
        }
    }
}
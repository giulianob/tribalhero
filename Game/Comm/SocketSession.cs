#region

using System;
using System.Net.Sockets;

#endregion

namespace Game.Comm
{
    public class SocketSession : Session
    {
        private readonly object writeLock = new object();

        public SocketSession(string name, Socket socket, IProcessor processor)
                : base(name, processor)
        {
            Socket = socket;
        }

        public Socket Socket { get; private set; }

        public override void Write(Packet packet)
        {
            lock (writeLock)
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Sending IP[{0}] {1}", Name, packet.ToString());
                }

                byte[] packetBytes = packet.GetBytes();
                int totalBytesSent = 0;

                var startTime = Environment.TickCount;

                try
                {
                    while (totalBytesSent < packetBytes.Length)
                    {
                        try
                        {
                            int bytesSent = Socket.Send(packetBytes, totalBytesSent, packetBytes.Length - totalBytesSent, SocketFlags.None);

                            // bytesSent 0 means there was an error sending the packet
                            if (bytesSent == 0)
                            {
                                break;
                            }

                            totalBytesSent += bytesSent;
                        }
                        catch(SocketException e)
                        {
                            if (e.SocketErrorCode == SocketError.WouldBlock)
                            {
                                try
                                {
                                    //if (Logger.IsDebugEnabled)
                                    //{
                                    Logger.Info(e, "Socket send buffer full. Setting it to blocking and trying again {0}", Name);
                                    //}

                                    Socket.Blocking = true;
                                }
                                catch(SocketException e2)
                                {
                                    Logger.Warn(e, "Socket unhandled exception when trying to block {0} {1}", e2.SocketErrorCode, e2.Message);
                                    return;
                                }
                                catch(Exception e2)
                                {
                                    Logger.Warn(e, "General unhandled exception when trying to block socket {0}", e2.Message);
                                }

                                continue;
                            }

                            switch(e.SocketErrorCode)
                            {
                                case SocketError.ConnectionReset:
                                case SocketError.TimedOut:
                                case SocketError.Shutdown:
                                    //if (Logger.IsDebugEnabled)
                                    //{
                                        Logger.Info(e, "Socket timeout/reset/shutdown handled packetLength[{0}] socketErrorCode[{1}] IP[{2}", packetBytes.Length, e.SocketErrorCode, Name);
                                    //}
                                    break;
                                default:
                                    Logger.Warn(e, "Socket exception with unhandled error {0} {1} {2}", e.SocketErrorCode, e.Message, Name);
                                    break;
                            }

                            return;
                        }
                        catch(ObjectDisposedException)
                        {
                            // This exception happens if the client disconnects and we shut down the socket but then still try to send data
                            return;
                        }
                        catch(Exception e)
                        {
                            Logger.Warn(e, "Unhandled exception when trying to send data to socket {0}", e.Message);
                            return;
                        }
                    }
                }
                finally
                {
                    try
                    {
                        if (Socket.Blocking)
                        {
                            Socket.Blocking = false;
                        }
                    }
                    catch(ObjectDisposedException)
                    {
                    }
                    catch(Exception e)
                    {
                        Logger.Warn(e, "Failed to reset socket blocking status");
                    }

                    var delta = Environment.TickCount - startTime;
                    if (delta > 200)
                    {
                        Logger.Info("Took {2}ms to send {1} bytes to {0}", Name, packetBytes.Length, delta);
                    }
                }
            }
        }
    }
}
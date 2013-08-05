#region

using System;
using System.Linq.Expressions;
using System.Net.Sockets;
using Game.Data;
using Game.Util;
using Ninject.Extensions.Logging;

#endregion

namespace Game.Comm
{
    public class SocketSession : Session
    {
        public SocketSession(string name, Socket socket, Processor processor)
                : base(name, processor)
        {
            Socket = socket;
        }

        public Socket Socket { get; private set; }

        public override bool Write(Packet packet)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Sending IP[{0}] {1}", Name, packet.ToString());
            }

            byte[] packetBytes = packet.GetBytes();
            int ret;

            while (true)
            {
                try
                {
                    ret = Socket.Send(packetBytes, packetBytes.Length, SocketFlags.None);
                    break;
                }
                catch(SocketException e)
                {
                    if (e.SocketErrorCode == SocketError.WouldBlock)
                    {
                        try
                        {
                            Socket.Blocking = true;
                        }
                        catch(Exception)
                        {                            
                            return false;
                        }

                        continue;
                    }
                    
                    if (e.SocketErrorCode == SocketError.TimedOut)
                    {
                        Logger.Warn(e, "Socket send timed out packetLength[{0}]", packetBytes.Length);
                    }
                    
                    return false;
                }
                catch(Exception)
                {
                    return false;
                }
            }

            try
            {
                if (Socket.Blocking)
                {
                    Socket.Blocking = false;
                }
            }
            catch(Exception e)
            {
                Logger.Warn(e, "Failed to reset socket blocking status");
            }

            return ret > 0;
        }
    }
}
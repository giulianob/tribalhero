#region

using System;
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
                        Socket.Blocking = true;
                        continue;
                    }
                    
                    return false;
                }
                catch(Exception)
                {
                    return false;
                }
            }

            if (Socket.Blocking)
            {
                Socket.Blocking = false;
            }

            return ret > 0;
        }
    }
}
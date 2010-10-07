#region

using System;
using System.Net.Sockets;
using Game.Data;

#endregion

namespace Game.Comm {
    public class SocketSession : Session {
        public Socket socket;

        public SocketSession(string name, Socket socket, Processor processor) : base(name, processor) {
            this.socket = socket;
        }

        protected override void Close() {
            try {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            } catch (Exception e) {
                Global.Logger.Warn(e);
            }
        }

        public override bool Write(Packet packet) {
#if DEBUG || CHECK_LOCKS
            Global.Logger.Info("Sending: " + packet.ToString(32));
#endif

            byte[] packetBytes = packet.GetBytes();
            int ret;
            if (socket == null)
                return false;

            try {
                ret = socket.Send(packetBytes, packetBytes.Length, SocketFlags.None);
            }
            catch (Exception) {
                return false;
            }
            return ret > 0;
        }
    }
}
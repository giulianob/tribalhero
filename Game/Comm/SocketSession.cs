#region

using System;
using System.Net.Sockets;

#endregion

namespace Game.Comm {
    public class SocketSession : Session {
        public Socket socket;

        public SocketSession(string name, Socket socket, Processor processor) : base(name, processor) {
            this.socket = socket;
        }

        protected override void close() {
            socket.Disconnect(false);
        }

        public override bool write(Packet packet) {
            Console.Out.WriteLine("Sending: " + packet.ToString(32));
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
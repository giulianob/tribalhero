using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
namespace Game.Comm {

    public class SocketSession : Session {
        public Socket socket;
        public SocketSession(string name, Socket socket, Processor processor)
            : base(name, processor) {
            this.socket = socket;
        }

        public override void close() {
            socket.Disconnect(false);
        }

        public override bool write(Packet packet) {
            Console.Out.WriteLine("Sending: " + packet.ToString());
            byte[] packetBytes = packet.getBytes();
            int ret;
            if (socket == null) return false;

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

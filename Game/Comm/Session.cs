#region

using System;
using System.IO;
using Game.Util;

#endregion

namespace Game.Comm {
    class PacketMaker {
        private MemoryStream ms = new MemoryStream();

        public void append(byte[] data) {
            ms.Write(data, 0, data.Length);
        }

        public Packet getNextPacket() {
            if (ms.Position < Packet.HEADER_SIZE)
                return null;
            int payload_len = BitConverter.ToUInt16(ms.GetBuffer(), Packet.LENGTH_OFFSET);
            if (payload_len > (ms.Position - Packet.HEADER_SIZE))
                return null;

            MemoryStream new_ms = new MemoryStream();
            int packet_len = Packet.HEADER_SIZE + payload_len;
            new_ms.Write(ms.GetBuffer(), packet_len, (int) ms.Position - packet_len);
            Packet packet = new Packet(ms.GetBuffer(), 0, packet_len);
            ms = new_ms;
            return packet;
        }
    }

    public abstract class Session : IChannel {
        public string name;
        private Player player = null;
        protected Processor processor;
        private PacketMaker packetMaker;

        public delegate void CloseCallback();

        public event CloseCallback OnClose;

        public Session(string name, Processor processor) {
            this.name = name;
            this.processor = processor;
            packetMaker = new PacketMaker();
        }

        public bool IsLoggedIn {
            get { return player != null; }
        }

        public Player Player {
            get { return player; }
            set { player = value; }
        }

        public abstract bool write(Packet packet);

        public void CloseSession() {
            if (OnClose != null)
                OnClose();
            close();
        }

        protected abstract void close();

        public void appendBytes(byte[] data) {
            packetMaker.append(data);
        }

        public Packet getNextPacket() {
            return packetMaker.getNextPacket();
        }

        public void process(object obj) {
            Packet p = (Packet) obj;

            if (!IsLoggedIn && p.Cmd != Command.LOGIN)
                return;
            else if (IsLoggedIn && p.Cmd == Command.LOGIN)
                return;

            if (processor != null)
                processor.Execute(this, p);
        }

        public void processEvent(object obj) {
            if (processor != null)
                processor.ExecuteEvent(this, (Packet) obj);
        }

        #region IChannel Members

        public void OnPost(object message) {
            write(message as Packet);
        }

        #endregion
    }
}
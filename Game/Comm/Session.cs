#region

using System;
using System.IO;
using Game.Util;

#endregion

namespace Game.Comm {
    class PacketMaker {
        private MemoryStream ms = new MemoryStream();

        public void Append(byte[] data) {
            ms.Write(data, 0, data.Length);
        }

        public Packet GetNextPacket() {
            if (ms.Position < Packet.HEADER_SIZE)
                return null;

            int payloadLen = BitConverter.ToUInt16(ms.GetBuffer(), Packet.LENGTH_OFFSET);
            if (payloadLen > (ms.Position - Packet.HEADER_SIZE))
                return null;

            MemoryStream newMs = new MemoryStream();
            int packetLen = Packet.HEADER_SIZE + payloadLen;
            newMs.Write(ms.GetBuffer(), packetLen, (int) ms.Position - packetLen);

            Packet packet;
            try {                
                packet = new Packet(ms.GetBuffer(), 0, packetLen);
            }
            catch (Exception) {
                packet = null;
            }

            ms = newMs;
            return packet;
        }
    }

    public abstract class Session : IChannel {
        public string name;
        private Player player;
        protected Processor processor;
        private readonly PacketMaker packetMaker;

        public delegate void CloseCallback();

        protected Session(string name, Processor processor) {
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

        public abstract bool Write(Packet packet);

        public void CloseSession() {
            Close();
        }

        protected abstract void Close();

        public void AppendBytes(byte[] data) {
            packetMaker.Append(data);
        }

        public Packet GetNextPacket() {
            return packetMaker.GetNextPacket();
        }

        public void Process(object obj) {
            Packet p = (Packet) obj;

            if (!IsLoggedIn && p.Cmd != Command.LOGIN)
                return;

            if (IsLoggedIn) {
                if (p.Cmd == Command.LOGIN)
                    return;

                if (player.GetCityList().Count == 0 && p.Cmd != Command.CITY_CREATE_INITIAL)
                    return;
            }

            if (processor != null)
                processor.Execute(this, p);
        }

        public void ProcessEvent(object obj) {
            if (processor != null)
                processor.ExecuteEvent(this, (Packet) obj);
        }

        #region IChannel Members

        public void OnPost(object message) {
            Write(message as Packet);
        }

        #endregion
    }
}
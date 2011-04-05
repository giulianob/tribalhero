#region

using System;
using System.IO;
using Game.Data;
using Game.Util;

#endregion

namespace Game.Comm
{
    public class PacketMaker
    {
        private MemoryStream ms = new MemoryStream();

        public long Length
        {
            get
            {
                return ms.Length;
            }
        }

        public void Append(byte[] data)
        {
            ms.Write(data, 0, data.Length);
        }

        public Packet GetNextPacket()
        {
            if (ms.Length < Packet.HEADER_SIZE)
                return null;

            byte[] payloadLengthBytes = new byte[sizeof(ushort)];
            ms.Position = Packet.LENGTH_OFFSET;
            ms.Read(payloadLengthBytes, 0, sizeof(ushort));
            ms.Seek(0, SeekOrigin.End);

            int payloadLen = BitConverter.ToUInt16(payloadLengthBytes, 0);
            int packetLen = Packet.HEADER_SIZE + payloadLen;

            if (ms.Length < packetLen)
                return null;            
            
            byte[] msBytes = ms.ToArray();

            var newMs = new MemoryStream();
            newMs.Write(msBytes, packetLen, msBytes.Length - packetLen);
            ms = newMs;

            Packet packet;
            try
            {
                packet = new Packet(msBytes, 0, packetLen);
            }
            catch(Exception)
            {
                packet = null;
            }
            
            return packet;
        }
    }

    public abstract class Session : IChannel
    {
        #region Delegates

        public delegate void CloseCallback(Session sender);

        public event CloseCallback OnClose;

        #endregion

        public PacketMaker PacketMaker { get; private set; }

        protected Processor processor;

        protected Session(string name, Processor processor)
        {
            Name = name;
            this.processor = processor;
            PacketMaker = new PacketMaker();
        }

        public string Name { get; private set; }

        public bool IsLoggedIn
        {
            get
            {
                return Player != null;
            }
        }

        public Player Player { get; set; }

        #region IChannel Members

        public void OnPost(object message)
        {
            Write(message as Packet);
        }

        #endregion

        public abstract bool Write(Packet packet);

        public void CloseSession()
        {
            Close();
        }

        protected void Close()
        {
            if (OnClose != null)
                OnClose(this);
        }

        public void Process(object obj)
        {
            var p = (Packet)obj;

            if (!IsLoggedIn && p.Cmd != Command.Login)
                return;

            if (IsLoggedIn)
            {
                if (p.Cmd == Command.Login)
                    return;

                if (Player.GetCityList().Count == 0 && p.Cmd != Command.CityCreateInitial)
                    return;
            }

            if (processor != null)
                processor.Execute(this, p);
        }

        public void ProcessEvent(object obj)
        {
            if (processor != null)
                processor.ExecuteEvent(this, (Packet)obj);
        }
    }
}
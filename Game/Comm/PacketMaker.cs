using System;
using System.IO;

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
            {
                return null;
            }

            byte[] payloadLengthBytes = new byte[sizeof(ushort)];
            ms.Position = Packet.LENGTH_OFFSET;
            ms.Read(payloadLengthBytes, 0, sizeof(ushort));
            ms.Seek(0, SeekOrigin.End);

            int payloadLen = BitConverter.ToUInt16(payloadLengthBytes, 0);
            int packetLen = Packet.HEADER_SIZE + payloadLen;

            if (ms.Length < packetLen)
            {
                return null;
            }

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
}
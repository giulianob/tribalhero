#region

using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using Game.Util;
using zlib;

#endregion

namespace Game.Comm
{
    public class Packet
    {
        #region Constants

        public enum Options : ushort
        {
            Compressed = 1,

            Failed = 2,

            Reply = 4
        }

        public const int HEADER_SIZE = 8;

        public const int LENGTH_OFFSET = 6;

        #endregion

        #region Packet Header

        private Command cmd = Command.Invalid;

        private ushort option;

        private ushort seq;

        public ushort Length { get; private set; }

        #endregion

        #region Members

        private readonly MemoryStream sendBuffer = new MemoryStream();

        private byte[] readBuffer;

        private ushort readOffset;

        #endregion

        #region Constructors

        public Packet()
        {            
        }

        public Packet(Command cmd)
        {
            this.cmd = cmd;
        }

        public Packet(Packet request)
        {
            cmd = request.cmd;
            seq = request.seq;
            option = (ushort)(request.option | (ushort)Options.Reply);
        }

        public Packet(byte[] data)
                : this(data, 0, data.Length)
        {
        }

        public Packet(byte[] data, int index, int count)
        {
            int dataLength = count - index;

            if (dataLength < HEADER_SIZE)
            {
                return;
            }

            seq = BitConverter.ToUInt16(data, 0);
            option = BitConverter.ToUInt16(data, 2);
            cmd = (Command)Enum.Parse(typeof(Command), BitConverter.ToUInt16(data, 4).ToString(CultureInfo.InvariantCulture), true);
            Length = BitConverter.ToUInt16(data, 6);

            readOffset = HEADER_SIZE;

            if (dataLength != HEADER_SIZE + Length)
            {
                return;
            }

            if ((option & (int)Options.Compressed) == (int)Options.Compressed)
            {
                var compressedMemory = new MemoryStream(data, index + HEADER_SIZE, count - HEADER_SIZE);
                var uncompressedMemory = new MemoryStream();
                compressedMemory.Position = 0;
                using (var ds = new DeflateStream(compressedMemory, CompressionMode.Decompress))
                {
                    int len;
                    var buff = new byte[4096];
                    while ((len = ds.Read(buff, 0, 4096)) > 0)
                    {
                        uncompressedMemory.Write(buff, 0, len);
                    }
                }

                readBuffer = new byte[HEADER_SIZE + uncompressedMemory.Length];
                Length = (ushort)readBuffer.Length;
                Array.Copy(data, index, readBuffer, 0, HEADER_SIZE); //copy header
                uncompressedMemory.Position = 0;
                uncompressedMemory.Read(readBuffer, HEADER_SIZE, (int)uncompressedMemory.Length);
            }
            else
            {
                readBuffer = new byte[count - index];
                Array.Copy(data, index, readBuffer, 0, count);
            }
        }

        #endregion

        #region Properties

        public Command Cmd
        {
            get
            {
                return cmd;
            }
            set
            {
                cmd = value;
            }
        }

        public ushort Seq
        {
            get
            {
                return seq;
            }
            set
            {
                seq = value;
            }
        }

        public ushort Option
        {
            get
            {
                return option;
            }
            set
            {
                option = value;
            }
        }

        #endregion

        #region Parameter Gets

        public void Reset()
        {
            readOffset = HEADER_SIZE;
        }

        public bool GetBoolean()
        {
            return GetByte() == 1;
        }

        public byte GetByte()
        {
            byte tmp = readBuffer[readOffset];
            readOffset += sizeof(byte);
            return tmp;
        }

        public byte[] GetBytes(ushort len)
        {
            var data = new byte[len];
            Array.Copy(readBuffer, readOffset, data, 0, len);
            readOffset += len;
            return data;
        }

        public short GetInt16()
        {
            short tmp = BitConverter.ToInt16(readBuffer, readOffset);
            readOffset += sizeof(short);
            return tmp;
        }

        public int GetInt32()
        {
            int tmp = BitConverter.ToInt32(readBuffer, readOffset);
            readOffset += sizeof(int);
            return tmp;
        }

        public ushort GetUInt16()
        {
            ushort tmp = BitConverter.ToUInt16(readBuffer, readOffset);
            readOffset += sizeof(ushort);
            return tmp;
        }

        public uint GetUInt32()
        {
            uint tmp = BitConverter.ToUInt32(readBuffer, readOffset);
            readOffset += sizeof(uint);
            return tmp;
        }

        public string GetString()
        {
            ushort len = BitConverter.ToUInt16(readBuffer, readOffset);
            readOffset += sizeof(ushort);
            string str = Encoding.UTF8.GetString(readBuffer, readOffset, len);
            readOffset += len;
            return str;
        }

        public float GetFloat()
        {
            float tmp = BitConverter.ToSingle(readBuffer, readOffset);
            readOffset += sizeof(float);
            return tmp;
        }

        #endregion

        #region Parameter Adds

        public void AddByte(byte value)
        {
            byte[] bytes = Parameter.ToBytes(value);
            sendBuffer.Write(bytes, 0, bytes.Length);
        }

        public void AddBytes(byte[] values)
        {
            sendBuffer.Write(values, 0, values.Length);
        }

        public void AddInt16(short value)
        {
            byte[] bytes = Parameter.ToBytes(value);
            sendBuffer.Write(bytes, 0, bytes.Length);
        }

        public void AddUInt16(ushort value)
        {
            byte[] bytes = Parameter.ToBytes(value);
            sendBuffer.Write(bytes, 0, bytes.Length);
        }

        public void AddUInt32(uint value)
        {
            byte[] bytes = Parameter.ToBytes(value);
            sendBuffer.Write(bytes, 0, bytes.Length);
        }

        public void AddInt32(int value)
        {
            byte[] bytes = Parameter.ToBytes(value);
            sendBuffer.Write(bytes, 0, bytes.Length);
        }

        public void AddInt64(long value)
        {
            byte[] bytes = Parameter.ToBytes(value);
            sendBuffer.Write(bytes, 0, bytes.Length);
        }

        public void AddString(string value)
        {
            byte[] bytes = Parameter.ToBytes(value);
            sendBuffer.Write(bytes, 0, bytes.Length);
        }

        public void AddFloat(float value)
        {
            byte[] bytes = Parameter.ToBytes(value);
            sendBuffer.Write(bytes, 0, bytes.Length);
        }

        public void AddBoolean(bool value)
        {
            AddByte((byte)(value ? 1 : 0));
        }

        #endregion

        #region Methods

        public byte[] GetPayload()
        {
            return sendBuffer.ToArray();
        }

        public byte[] GetBytes()
        {
            byte[] ret;

            if (readBuffer != null)
            {
                return readBuffer;
            }

            using (var memory = new MemoryStream())
            {
                //write header
                var binaryWriter = new BinaryWriter(memory);
                binaryWriter.Write(seq);
                binaryWriter.Write(option);
                binaryWriter.Write((ushort)cmd);
                binaryWriter.Write(UInt16.MinValue); //place holder for length

                if ((option & (int)Options.Compressed) == (int)Options.Compressed)
                {
                    using (var compressed = new MemoryStream())
                    {
                        using (var ds = new ZOutputStream(compressed, 3))
                        {
                            sendBuffer.Position = 0;
                            ds.Write(sendBuffer.ToArray(), 0, (int)sendBuffer.Length);
                            ds.finish();
                            compressed.Position = 0;
                            compressed.WriteTo(memory);
                        }
                    }
                }
                else
                {
                    binaryWriter.Write(sendBuffer.ToArray());
                }

                var len = (ushort)(memory.Length - HEADER_SIZE);
                binaryWriter.Seek(LENGTH_OFFSET, SeekOrigin.Begin);
                binaryWriter.Write(len);

                ret = new byte[memory.Length];
                memory.Position = 0;
                memory.Read(ret, 0, (int)memory.Length);
            }

            readBuffer = ret;

            return ret;
        }

        public string ToString(int maxLength = -1)
        {
            string str = string.Empty;
            
            if (sendBuffer.Length <= maxLength)
            {
                byte[] dump = GetBytes();
                str = HexDump.GetString(dump, 8, 16);
            }

            return string.Format("Cmd[{0}] Seq[{1}] Reply[{2}] ReadBufferLen[{3}] SendBufferLen[{4}]:{5}{6}", 
                cmd, 
                seq, 
                (option & (int)Options.Reply) == (int)Options.Reply, 
                readBuffer != null ? readBuffer.Length.ToString(CultureInfo.InvariantCulture) : "N/A",
                sendBuffer.Length,                 
                string.IsNullOrEmpty(str) ? string.Empty : Environment.NewLine,
                str);
        }

        #endregion
    }
}
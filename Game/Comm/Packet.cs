#region

using System;
using System.Collections.Generic;
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

        private readonly byte[] bytes;
        private readonly List<Parameter> parameters = new List<Parameter>();
        private ushort offset;

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

        public Packet(byte[] data) : this(data, 0, data.Length)
        {
        }

        public Packet(byte[] data, int index, int count)
        {
            int dataLength = count - index;

            if (dataLength < HEADER_SIZE)
                return;

            seq = BitConverter.ToUInt16(data, 0);
            option = BitConverter.ToUInt16(data, 2);
            cmd = (Command)Enum.Parse(typeof(Command), BitConverter.ToUInt16(data, 4).ToString(), true);
            Length = BitConverter.ToUInt16(data, 6);

            offset = HEADER_SIZE;

            if (dataLength != HEADER_SIZE + Length)
                return;

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
                        uncompressedMemory.Write(buff, 0, len);
                }

                bytes = new byte[HEADER_SIZE + uncompressedMemory.Length];
                Length = (ushort)bytes.Length;
                Array.Copy(data, index, bytes, 0, HEADER_SIZE); //copy header
                uncompressedMemory.Position = 0;
                uncompressedMemory.Read(bytes, HEADER_SIZE, (int)uncompressedMemory.Length);
            }
            else
            {
                bytes = new byte[count - index];
                Array.Copy(data, index, bytes, 0, count);
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

        public bool Empty
        {
            get
            {
                return bytes == null || offset == bytes.Length;
            }
        }

        #endregion

        #region Parameter Gets

        public void Reset()
        {
            offset = HEADER_SIZE;
        }

        public byte GetByte()
        {
            byte tmp = bytes[offset];
            offset += sizeof(byte);
            return tmp;
        }

        public byte[] GetBytes(ushort len)
        {
            var data = new byte[len];
            Array.Copy(bytes, offset, data, 0, len);
            offset += len;
            return data;
        }

        public short GetInt16()
        {
            short tmp = BitConverter.ToInt16(bytes, offset);
            offset += sizeof(short);
            return tmp;
        }

        public int GetInt32()
        {
            int tmp = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);
            return tmp;
        }

        public ushort GetUInt16()
        {
            ushort tmp = BitConverter.ToUInt16(bytes, offset);
            offset += sizeof(ushort);
            return tmp;
        }

        public uint GetUInt32()
        {
            uint tmp = BitConverter.ToUInt32(bytes, offset);
            offset += sizeof(uint);
            return tmp;
        }

        public string GetString()
        {
            ushort len = BitConverter.ToUInt16(bytes, offset);
            offset += sizeof(ushort);
            string str = Encoding.UTF8.GetString(bytes, offset, len);
            offset += len;
            return str;
        }

        public float GetFloat()
        {
            float tmp = BitConverter.ToSingle(bytes, offset);
            offset += sizeof(float);
            return tmp;
        }

        #endregion

        #region Parameter Adds

        public void AddParamater(Parameter parameter)
        {
            parameters.Add(parameter);
        }

        public void AddByte(byte value)
        {
            parameters.Add(new Parameter(value));
        }

        public void AddBytes(byte[] values)
        {
            parameters.Add(new Parameter(values));
        }

        public void AddInt16(short value)
        {
            parameters.Add(new Parameter(value));
        }

        public void AddUInt16(ushort value)
        {
            parameters.Add(new Parameter(value));
        }

        public void AddUInt32(uint value)
        {
            parameters.Add(new Parameter(value));
        }

        public void AddInt32(int value)
        {
            parameters.Add(new Parameter(value));
        }

        public void AddInt64(long value)
        {
            parameters.Add(new Parameter(value));
        }

        public void AddString(string value)
        {
            parameters.Add(new Parameter(value));
        }

        public void AddFloat(float value)
        {
            parameters.Add(new Parameter(value));
        }

        #endregion

        #region Methods

        public byte[] GetBytes()
        {
            byte[] ret;

            if (bytes != null)
                return bytes;

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
                            foreach (var param in parameters)
                            {
                                byte[] paramBytes = param.GetBytes();
                                ds.Write(param.GetBytes(), 0, paramBytes.Length);
                            }
                            ds.finish();
                            compressed.Position = 0;
                            compressed.WriteTo(memory);
                        }
                    }
                }
                else
                {
                    foreach (var param in parameters)
                    {
                        byte[] paramBytes = param.GetBytes();
                        binaryWriter.Write(paramBytes);
                    }
                }

                var len = (ushort)(memory.Length - HEADER_SIZE);
                binaryWriter.Seek(LENGTH_OFFSET, SeekOrigin.Begin);
                binaryWriter.Write(len);

                ret = new byte[memory.Length];
                memory.Position = 0;
                memory.Read(ret, 0, (int)memory.Length);
            }

            return ret;
        }

        public string ToString(int maxLength)
        {
            string str = "<Too Large To Display>";
            byte[] dump = GetBytes();

            if (dump.Length <= maxLength)
                str = HexDump.GetString(dump, 8, 16);

            return "Cmd[" + cmd + "] Len[" + dump.Length + "]:" + Environment.NewLine + str;
        }

        #endregion
    }
}
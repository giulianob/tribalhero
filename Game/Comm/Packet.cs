using System;
using System.Text;
using System.Collections;
using Game.Util;
using System.IO.Compression;
using System.IO;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;

namespace Game.Comm {
    public class Packet {
        #region Constants
        public enum Options : ushort {
            COMPRESSED = 1,
            FAILED = 2,
            REPLY = 4
        }

        public const int HEADER_SIZE = 8;
        public const int LENGTH_OFFSET = 6;
        #endregion

        #region Packet Header
        private ushort seq = 0;
        private ushort option = 0;
        private Command cmd = Command.INVALID;
        private ushort length = 0;

        public ushort Length {
            get { return length; }
        }
        #endregion

        #region Members
        ushort offset = 0;
        byte[] bytes = null;
        List<Parameter> parameters = new List<Parameter>();
        #endregion

        #region Constructors
        public Packet() {
        }

        public Packet(Command cmd) {
            this.cmd = cmd;
        }

        public Packet(Packet request) {
            this.cmd = request.cmd;
            this.seq = request.seq;
            this.option = (ushort)((ushort)request.option | (ushort)Options.REPLY);
        }

        public Packet(byte[] data)
            :
            this(data, 0, data.Length) {

        }

        public Packet(byte[] data, int index, int count) {
            int dataLength = count - index;

            if (dataLength < HEADER_SIZE)
                return;

            this.seq = BitConverter.ToUInt16(data, 0);
            this.option = BitConverter.ToUInt16(data, 2);
            this.cmd = (Command)Enum.Parse(typeof(Command), BitConverter.ToUInt16(data, 4).ToString(), true);
            this.length = BitConverter.ToUInt16(data, 6);

            offset = HEADER_SIZE;

            if (dataLength != HEADER_SIZE + length)
                return;

            if ((option & (int)Options.COMPRESSED) == (int)Options.COMPRESSED) {
                MemoryStream compressedMemory = new MemoryStream(data, index + HEADER_SIZE, count - HEADER_SIZE);
                MemoryStream uncompressedMemory = new MemoryStream();
                compressedMemory.Position = 0;
                GZipStream gs = new GZipStream(compressedMemory, CompressionMode.Decompress, false);
                //      ZInputStream gs = new ZInputStream(compressedMemory);

                int len = 0;
                byte[] buff = new byte[4096];
                while ((len = gs.Read(buff, 0, 4096)) > 0)
                    uncompressedMemory.Write(buff, 0, len);

                gs.Close();

                this.bytes = new byte[HEADER_SIZE + uncompressedMemory.Length];
                this.length = (ushort)bytes.Length;
                Array.Copy(data, index, this.bytes, 0, HEADER_SIZE); //copy header
                uncompressedMemory.Position = 0;
                uncompressedMemory.Read(this.bytes, HEADER_SIZE, (int)uncompressedMemory.Length);
            }
            else {
                this.bytes = new byte[count - index];
                Array.Copy(data, index, this.bytes, 0, count);
            }
        }
        #endregion

        #region Properties
        public Command Cmd {
            get { return cmd; }
            set { cmd = value; }
        }

        public ushort Seq {
            get { return seq; }
            set { seq = value; }
        }

        public ushort Option {
            get { return option; }
            set { option = value; }
        }

        public bool Empty {
            get { return bytes == null || offset == bytes.Length; }
        }

        #endregion

        #region Parameter Gets
        public void reset() {
            offset = HEADER_SIZE;
        }

        public byte getByte() {
            byte tmp = bytes[offset];
            offset += sizeof(byte);
            return tmp;
        }
        public byte[] getBytes(ushort len) {
            byte[] data = new byte[len];
            Array.Copy(bytes, offset, data, 0, len);
            offset += len;
            return data;
        }
        public int getInt16() {
            int tmp = BitConverter.ToInt16(bytes, offset);
            offset += sizeof(int);
            return tmp;
        }

        public ushort getUInt16() {
            ushort tmp = BitConverter.ToUInt16(bytes, offset);
            offset += sizeof(ushort);
            return tmp;
        }

        public uint getUInt32() {
            uint tmp = BitConverter.ToUInt32(bytes, offset);
            offset += sizeof(uint);
            return tmp;
        }

        public string getString() {
            ushort len = BitConverter.ToUInt16(bytes, offset);
            offset += sizeof(ushort);
            string str = Encoding.UTF8.GetString(bytes, offset, len);
            offset += len;
            return str;
        }
        #endregion

        #region Parameter Adds
        public void addParamater(Parameter parameter) {
            parameters.Add(parameter);
        }
        public void addByte(byte value) {
            parameters.Add(new Parameter(value));
        }
        public void addBytes(byte[] values) {
            parameters.Add(new Parameter(values));
        }
        public void addInt16(int value) {
            parameters.Add(new Parameter(value));
        }

        public void addUInt16(ushort value) {
            parameters.Add(new Parameter(value));
        }

        public void addUInt32(uint value) {
            parameters.Add(new Parameter(value));
        }
        public void addInt32(int value) {
            parameters.Add(new Parameter(value));
        }
        public void addInt64(long value) {
            parameters.Add(new Parameter(value));
        }
        public void addString(string value) {
            parameters.Add(new Parameter(value));
        }
        #endregion

        #region Methods
        public byte[] getBytes() {
            byte[] ret = null;

            if (bytes != null)
                return bytes;

            using (MemoryStream memory = new MemoryStream()) {
                //write header
                BinaryWriter binaryWriter = new BinaryWriter(memory);
                binaryWriter.Write(seq);
                binaryWriter.Write(option);
                binaryWriter.Write((ushort)cmd);
                binaryWriter.Write(UInt16.MinValue); //place holder for length

                if ((option & (int)Options.COMPRESSED) == (int)Options.COMPRESSED) {
                    MemoryStream compressedMemory = new MemoryStream();
                    //         ZOutputStream gs = new ZOutputStream(memory, zlibConst.Z_DEFAULT_COMPRESSION,true);
                    GZipStream gs = new GZipStream(memory, CompressionMode.Compress, true);

                    foreach (Parameter param in parameters) {
                        byte[] paramBytes = param.getBytes();
                        gs.Write(param.getBytes(), 0, paramBytes.Length);
                    }

                    gs.Close();
                    compressedMemory.WriteTo(memory);
                }
                else {
                    foreach (Parameter param in parameters) {
                        byte[] paramBytes = param.getBytes();
                        binaryWriter.Write(param.getBytes());
                    }
                }

                ushort len = (ushort)(memory.Length - HEADER_SIZE);
                binaryWriter.Seek(LENGTH_OFFSET, SeekOrigin.Begin);
                binaryWriter.Write(len);

                ret = new byte[memory.Length];
                memory.Position = 0;
                memory.Read(ret, 0, (int)memory.Length);
            }

            return ret;
        }

        public string ToString(int maxLength) {
            string str = "<Too Large To Display>";
            byte[] dump = getBytes();

            if (dump.Length <= maxLength)
                str = HexDump.GetString(dump, 8, 16);

            return "Cmd[" + cmd + "] Len[" + dump.Length + "]:" + Environment.NewLine + str;
        }

        #endregion




    }
}

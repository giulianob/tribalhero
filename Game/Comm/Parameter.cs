using System;
using System.Collections.Generic;
using System.Text;

namespace Game.Comm {
    public class Parameter {
        object value = null;
        int length = 0;
        byte[] bytes = null;

        public Parameter(int value) {
            this.value = value;
            this.length = sizeof(int);
            this.bytes = BitConverter.GetBytes(value);
        }
        public Parameter(long value) {
            this.value = value;
            this.length = sizeof(long);
            this.bytes = BitConverter.GetBytes(value);
        }
        public Parameter(uint value) {
            this.value = value;
            this.length = sizeof(uint);
            this.bytes = BitConverter.GetBytes(value);
        }

        public Parameter(ushort value) {
            this.value = value;
            this.length = sizeof(ushort);
            this.bytes = BitConverter.GetBytes(value);
        }

        public Parameter(byte value) {
            this.value = value;
            this.length = sizeof(byte);
            this.bytes = new byte[1] { value };
        }

        public Parameter(string value) {
            this.value = value;
            this.length = sizeof(ushort) + value.Length;
            this.bytes = new byte[2 + length];
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)this.length), 0, this.bytes, 0, 2);
            Buffer.BlockCopy(Encoding.UTF8.GetBytes(value), 0, this.bytes, 2, value.Length);
        }

        public Parameter(byte[] value) {
            this.value = value;
            this.length = value.Length;
            this.bytes = value;
        }

        public object Value {
            get { return value; }
        }

        public int Length {
            get { return length; }
        }

        public byte[] getBytes() {
            return bytes;
        }
    }
}
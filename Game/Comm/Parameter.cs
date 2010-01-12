#region

using System;
using System.Text;

#endregion

namespace Game.Comm {
    public class Parameter {
        private object value = null;
        private int length = 0;
        private byte[] bytes = null;

        public Parameter(int value) {
            this.value = value;
            length = sizeof (int);
            bytes = BitConverter.GetBytes(value);
        }

        public Parameter(long value) {
            this.value = value;
            length = sizeof (long);
            bytes = BitConverter.GetBytes(value);
        }

        public Parameter(uint value) {
            this.value = value;
            length = sizeof (uint);
            bytes = BitConverter.GetBytes(value);
        }

        public Parameter(ushort value) {
            this.value = value;
            length = sizeof (ushort);
            bytes = BitConverter.GetBytes(value);
        }

        public Parameter(byte value) {
            this.value = value;
            length = sizeof (byte);
            bytes = new byte[1] {value};
        }

        public Parameter(string value) {
            this.value = value;
            length = sizeof (ushort) + value.Length;
            bytes = new byte[2 + length];
            Buffer.BlockCopy(BitConverter.GetBytes((ushort) length), 0, bytes, 0, 2);
            Buffer.BlockCopy(Encoding.UTF8.GetBytes(value), 0, bytes, 2, value.Length);
        }

        public Parameter(byte[] value) {
            this.value = value;
            length = value.Length;
            bytes = value;
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
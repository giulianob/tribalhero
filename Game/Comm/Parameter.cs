#region

using System;
using System.Text;

#endregion

namespace Game.Comm
{
    public class Parameter
    {
        private readonly byte[] bytes;

        public Parameter(int value)
        {
            Value = value;
            Length = sizeof(int);
            bytes = BitConverter.GetBytes(value);
        }

        public Parameter(short value)
        {
            Value = value;
            Length = sizeof(short);
            bytes = BitConverter.GetBytes(value);
        }

        public Parameter(long value)
        {
            Value = value;
            Length = sizeof(long);
            bytes = BitConverter.GetBytes(value);
        }

        public Parameter(uint value)
        {
            Value = value;
            Length = sizeof(uint);
            bytes = BitConverter.GetBytes(value);
        }

        public Parameter(ushort value)
        {
            Value = value;
            Length = sizeof(ushort);
            bytes = BitConverter.GetBytes(value);
        }

        public Parameter(byte value)
        {
            Value = value;
            Length = sizeof(byte);
            bytes = new[] {value};
        }

        public Parameter(float value)
        {
            Value = value;
            Length = sizeof(float);
            bytes = BitConverter.GetBytes(value);
        }

        public Parameter(string value)
        {
            Value = value;
            Length = sizeof(ushort) + value.Length;
            bytes = new byte[2 + Length];
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)Length), 0, bytes, 0, 2);
            Buffer.BlockCopy(Encoding.UTF8.GetBytes(value), 0, bytes, 2, value.Length);
        }

        public Parameter(byte[] value)
        {
            Value = value;
            Length = value.Length;
            bytes = value;
        }

        public object Value { get; private set; }

        public int Length { get; private set; }

        public byte[] GetBytes()
        {
            return bytes;
        }
    }
}
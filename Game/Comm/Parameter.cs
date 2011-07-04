#region

using System;
using System.Text;

#endregion

namespace Game.Comm
{
    public class Parameter
    {        
        public static byte[] ToBytes(int value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] ToBytes(short value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] ToBytes(long value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] ToBytes(uint value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] ToBytes(ushort value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] ToBytes(byte value)
        {
            return new[] {value};
        }

        public static byte[] ToBytes(float value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] ToBytes(string value)
        {
            int length = sizeof(ushort) + value.Length;
            byte[] bytes = new byte[2 + length];
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)length), 0, bytes, 0, 2);
            Buffer.BlockCopy(Encoding.UTF8.GetBytes(value), 0, bytes, 2, value.Length);

            return bytes;
        }
    }
}
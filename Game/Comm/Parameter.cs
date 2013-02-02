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
            byte[] stringBytes = Encoding.UTF8.GetBytes(value);
            int length = sizeof(ushort) + stringBytes.Length;
            byte[] bytes = new byte[2 + length];
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)length), 0, bytes, 0, 2);
            Buffer.BlockCopy(stringBytes, 0, bytes, 2, stringBytes.Length);

            return bytes;
        }
    }
}
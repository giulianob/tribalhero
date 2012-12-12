#region

using System;
using System.IO;
using System.Text;

#endregion

namespace Game.Util
{
    struct HexByteArray
    {
        private const string MAP = "0123456789ABCDEF";

        private readonly byte[] arr;

        private readonly int sz;

        public HexByteArray(byte[] arr, int s)
        {
            this.arr = arr;
            sz = s;
        }

        public override string ToString()
        {
            var str = new StringBuilder();
            int count = 0;
            foreach (var b in arr)
            {
                if (count == sz / 2)
                {
                    str.Append("| ");
                }
                char lsb = MAP[(b & 0x0f)];
                char msb = MAP[((b >> 4) & 0x0f)];
                str.Append(msb);
                str.Append(lsb);
                str.Append(' ');
                count++;
            }
            return str.ToString();
        }
    }

    struct PritableByteArray
    {
        private readonly byte[] arr;

        public PritableByteArray(byte[] arr)
        {
            this.arr = arr;
        }

        public override string ToString()
        {
            var str = new StringBuilder();
            foreach (var b in arr)
            {
                char ch = '.';
                if (!(Char.IsWhiteSpace((char)b) || Char.IsControl((char)b)))
                {
                    ch = (char)b;
                }
                str.Append(ch);
            }

            return str.ToString();
        }
    }

    public class HexDump
    {
        public static string GetString(byte[] bytes, int offset)
        {
            return GetString(bytes, offset, 20);
        }

        public static string GetString(byte[] bytes, int offset, int sz)
        {
            var buf = new byte[sz];
            int len;

            var ret = new StringBuilder();
            var ms = new MemoryStream(bytes);
            ms.Seek(offset, SeekOrigin.Begin);
            while ((len = ms.Read(buf, 0, buf.Length)) > 0)
            {
                byte[] tmp = buf;
                if (len < buf.Length)
                {
                    tmp = new byte[len];

                    for (int i = 0; i < tmp.Length; i++)
                    {
                        tmp[i] = buf[i];
                    }
                }
                var hex = new HexByteArray(tmp, sz);
                var asc = new PritableByteArray(tmp);
                ret.Append(hex.ToString().PadRight(3 * sz + 2, ' ') + " " + asc + Environment.NewLine);
            }

            return ret.ToString();
        }
    }
}
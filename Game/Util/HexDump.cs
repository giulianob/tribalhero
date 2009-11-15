using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Game.Util {
    struct HexByteArray {
        private byte[] arr;
        private int sz;

        public HexByteArray(byte[] arr, int s) {
            this.arr = arr;
            this.sz = s;
        }

        override public string ToString() {
            StringBuilder str = new StringBuilder();
            int count = 0;
            foreach (byte b in arr) {
                if (count == sz / 2) str.Append("| ");
                char lsb = map[(b & 0x0f)];
                char msb = map[((b >> 4) & 0x0f)];
                str.Append(msb);
                str.Append(lsb);
                str.Append(' ');
                count++;
            }
            return str.ToString();
        }
        private const string map = "0123456789ABCDEF";
    }

    struct PritableByteArray {
        private byte[] arr;

        public PritableByteArray(byte[] arr) {
            this.arr = arr;
        }

        override public string ToString() {
            StringBuilder str = new StringBuilder();
            foreach (byte b in arr) {
                char ch = '.';
                if (!(Char.IsWhiteSpace((char)b) || Char.IsControl((char)b)))
                    ch = (char)b;
                str.Append(ch);
            }

            return str.ToString();
        }
    }


    public class HexDump {

        public static string GetString(byte[] bytes, int offset) {
            return GetString(bytes, offset, 20);
        }

        public static string GetString(byte[] bytes, int offset, int sz) {
            byte[] buf = new byte[sz];
            int len;

            StringBuilder ret = new StringBuilder();
            MemoryStream ms = new MemoryStream(bytes);
            ms.Seek(offset, SeekOrigin.Begin);
            int totLen = 0;
            while ((len = ms.Read(buf, 0, buf.Length)) > 0) {
                totLen += len;
                byte[] tmp = buf;
                if (len < buf.Length) {
                    tmp = new byte[len];

                    for (int i = 0; i < tmp.Length; i++)
                        tmp[i] = buf[i];
                }
                HexByteArray hex = new HexByteArray(tmp, sz);
                PritableByteArray asc = new PritableByteArray(tmp);
                ret.Append(hex.ToString().PadRight(3 * sz + 2, ' ') + " " + asc + System.Environment.NewLine);
            }

            return ret.ToString();
        }
    }
}

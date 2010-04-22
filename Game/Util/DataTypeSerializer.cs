#region

using System;

#endregion

namespace Game.Util {
    class DataTypeSerializer {
        public static byte Serialize(object value) {
            if (value is uint)
                return 1;
            else if (value is ushort)
                return 2;
            else if (value is byte)
                return 3;
            else if (value is string)
                return 4;
            else if (value is short)
                return 5;
            else if (value is int)
                return 6;
            else if (value is DateTime)
                return 7;

            throw new Exception("Unknown data type");
        }

        public static object Deserialize(string value, byte datatype) {
            switch (datatype) {
                case 1:
                    return uint.Parse(value);
                case 2:
                    return ushort.Parse(value);
                case 3:
                    return byte.Parse(value);
                case 4:
                    return value;
                case 5:
                    return short.Parse(value);
                case 6:
                    return int.Parse(value);
                case 7:
                    return DateTime.Parse(value);
                default:
                    throw new Exception("Unknown data type");
            }
        }
    }
}
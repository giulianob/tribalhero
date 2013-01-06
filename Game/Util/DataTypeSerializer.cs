#region

using System;

#endregion

namespace Game.Util
{
    class DataTypeSerializer
    {
        public static byte Serialize(object value)
        {
            if (value is uint)
            {
                return 1;
            }
            if (value is ushort)
            {
                return 2;
            }
            if (value is byte)
            {
                return 3;
            }
            if (value is string)
            {
                return 4;
            }
            if (value is short)
            {
                return 5;
            }
            if (value is int)
            {
                return 6;
            }
            if (value is DateTime)
            {
                return 7;
            }
            if (value is long)
            {
                return 8;
            }
            if (value is float)
            {
                return 9;
            }

            throw new Exception("Unknown data type");
        }

        public static object Deserialize(string value, byte datatype)
        {
            switch(datatype)
            {
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
                    return DateTime.SpecifyKind(DateTime.Parse(value), DateTimeKind.Utc);
                case 8:
                    return long.Parse(value);
                case 9:
                    return float.Parse(value);
                default:
                    throw new Exception("Unknown data type");
            }
        }
    }
}
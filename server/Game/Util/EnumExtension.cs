using System;

namespace Game.Util
{
    public class EnumExtension
    {
        public static TEnum Parse<TEnum>(string value, bool ignoreCase = true) where TEnum : struct
        {
            return (TEnum)Enum.Parse(typeof(TEnum), value, ignoreCase);
        }
    }
}
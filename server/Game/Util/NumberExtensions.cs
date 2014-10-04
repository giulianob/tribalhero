using System;

namespace Game.Util
{
    public static class NumberExtensions
    {
        public static decimal FromPercentageDiscount(this int value)
        {
            return (100m - value) / 100m;
        }

        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0)
            {
                return min;
            }

            return val.CompareTo(max) > 0 ? max : val;
        }
    }
}

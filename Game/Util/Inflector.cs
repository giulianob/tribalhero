#region

using System;
using System.Globalization;

#endregion

namespace Game.Util
{
    public static class Inflector
    {
        public static string ToUpperWords(this String str)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str);
        }

        public static string ToCamelCase(this String str)
        {
            return str.Replace('_', ' ').ToUpperWords().Replace(" ", "");
        }
    }
}
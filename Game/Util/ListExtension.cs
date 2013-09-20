using System;
using System.Collections.Generic;

namespace Game.Util
{
    public static class ListExtension
    {
        /// <summary>
        /// Implements ‘random’ shuffling, Fisher-Yates style
        /// </summary>
        public static List<T> Shuffle<T>(this List<T> list, int seed)
        {
            Random rng = new Random(seed);
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }
    }
}

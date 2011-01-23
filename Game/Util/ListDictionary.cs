#region

using System;
using System.Collections.Generic;

#endregion

namespace Game.Util
{
    public class ListDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public List<TValue> FindAll(Predicate<TValue> match)
        {
            var matches = new List<TValue>();

            ValueCollection.Enumerator ite = Values.GetEnumerator();

            while (ite.MoveNext())
            {
                if (match(ite.Current))
                    matches.Add(ite.Current);
            }

            return matches;
        }

        public bool Exists(Predicate<TValue> match)
        {
            ValueCollection.Enumerator ite = Values.GetEnumerator();

            while (ite.MoveNext())
            {
                if (match(ite.Current))
                    return true;
            }

            return false;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace Game.Util {
    public class ListDictionary<TKey, TValue> : Dictionary<TKey, TValue> {

        public List<TValue> FindAll(Predicate<TValue> match) {
            List<TValue> matches = new List<TValue>();

            Dictionary<TKey, TValue>.ValueCollection.Enumerator ite = Values.GetEnumerator();

            while (ite.MoveNext()) {
                if (match(ite.Current))
                    matches.Add(ite.Current);
            }

            return matches;
        }

        public bool Exists(Predicate<TValue> match) {
            Dictionary<TKey, TValue>.ValueCollection.Enumerator ite = Values.GetEnumerator();

            while (ite.MoveNext()) {
                if (match(ite.Current))
                    return true;
            }

            return false;
        }
    }
}

#region

using System;
using System.Collections.Generic;
using Game.Data;

#endregion

namespace Game.Database {
    public class PersistableList<T> : List<T> where T : IPersistableObject {
        public PersistableList() {}

        public new void Add(T item) {
            Add(item, true);
        }

        public void Add(T item, bool save) {
            base.Add(item);
            if (save)
                Global.dbManager.Save(item);
        }

        public new bool Remove(T item) {
            return Remove(item, true);
        }

        public bool Remove(T item, bool save) {
            if (base.Remove(item)) {
                if (save)
                    Global.dbManager.Delete(item);
                return true;
            } else
                return false;
        }

        public new void RemoveAt(int index) {
            Global.dbManager.Delete(this[index]);
            base.RemoveAt(index);
        }

        public new int RemoveAll(Predicate<T> match) {
            int count = 0;
            for (int i = Count - 1; i >= 0; i--) {
                if (match(this[i])) {
                    RemoveAt(i);
                    count++;
                }
            }

            return count;
        }

        public new void Clear() {
            foreach (IPersistableObject item in this)
                Global.dbManager.Delete(item);

            base.Clear();
        }
    }
}
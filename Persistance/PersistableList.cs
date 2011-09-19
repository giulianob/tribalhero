#region

using System;
using System.Collections.Generic;

#endregion

namespace Persistance
{
    abstract public class PersistableList<T> : List<T> where T : IPersistableObject
    {
        protected IDbManager manager;

        protected PersistableList(IDbManager manager)
        {
            this.manager = manager;
        }

        public new void Add(T item)
        {
            Add(item, true);
        }

        public void Add(T item, bool save)
        {
            base.Add(item);
            if (save)
                manager.Save(item);
        }

        public new bool Remove(T item)
        {
            return Remove(item, true);
        }

        public bool Remove(T item, bool save)
        {
            if (base.Remove(item))
            {
                if (save)
                    manager.Delete(item);

                return true;
            }
            
            return false;
        }

        public new void RemoveAt(int index)
        {
            manager.Delete(this[index]);
            base.RemoveAt(index);
        }

        public new int RemoveAll(Predicate<T> match)
        {
            int count = 0;
            for (int i = Count - 1; i >= 0; i--)
            {
                if (!match(this[i]))
                    continue;

                RemoveAt(i);
                count++;
            }

            return count;
        }

        public new void Clear()
        {
            foreach (T item in this)
                manager.Delete(item);

            base.Clear();
        }
    }
}
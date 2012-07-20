#region

using System;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace Persistance
{
    abstract public class ListOfPersistableObjects<T> : IListOfPersistableObjects<T> where T : IPersistableObject
    {
        private readonly IDbManager manager;

        /// <summary>
        /// Do not modify the backing list from subclasses directly!
        /// </summary>
        protected readonly List<T> BackingList; 

        protected ListOfPersistableObjects(IDbManager manager)
        {
            BackingList = new List<T>();
            this.manager = manager;
        }

        public void Add(T item)
        {
            Add(item, true);
        }

        public void Add(T item, bool save)
        {
            BackingList.Add(item);
            if (save)
                manager.Save(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            BackingList.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return Remove(item, true);
        }

        public int Count
        {
            get
            {
                return BackingList.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool Remove(T item, bool save)
        {
            if (BackingList.Remove(item))
            {
                if (save)
                    manager.Delete(item);

                return true;
            }
            
            return false;
        }

        public int IndexOf(T item)
        {
            return BackingList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            BackingList.Insert(index, item);
            manager.Save(item);
        }

        public void RemoveAt(int index)
        {
            manager.Delete(this[index]);
            BackingList.RemoveAt(index);
        }

        public T this[int index]
        {
            get
            {
                return BackingList[index];
            }
            set
            {
                manager.Delete(BackingList[index]);
                BackingList[index] = value;
                manager.Save(value);
            }
        }

        public int RemoveAll(Predicate<T> match)
        {
            // We implement our own RemoveAll because it needs to call RemoveAt which removes the item from the db as well
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

        public void Clear()
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                RemoveAt(i);
            }
        }

        public bool Contains(T item)
        {
            return BackingList.Contains(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return BackingList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
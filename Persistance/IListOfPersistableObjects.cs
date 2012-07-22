using System.Collections.Generic;

namespace Persistance
{
    public interface IListOfPersistableObjects<T> : IList<T> where T : IPersistableObject
    {
        void Add(T item, bool save);

        bool Remove(T item, bool save);
    }
}
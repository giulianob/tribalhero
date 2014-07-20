using System.Collections.Generic;

namespace Game.Data.Store
{
    public interface IStoreManager
    {
        IEnumerable<StoreItem> Items { get; }

        int ItemsCount { get; }

        void UpdateItems(IEnumerable<StoreItem> newItems);
    }
}
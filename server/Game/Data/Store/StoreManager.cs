using System.Collections.Generic;
using System.Linq;

namespace Game.Data.Store
{
    public class StoreManager : IStoreManager
    {
        private List<StoreItem> items;

        public IEnumerable<StoreItem> Items
        {
            get
            {
                return items.AsEnumerable();
            }
        }

        public int ItemsCount
        {
            get
            {
                return items.Count;
            }
        }

        public void UpdateItems(IEnumerable<StoreItem> newItems)
        {
            this.items = newItems.ToList();
        }
    }
}
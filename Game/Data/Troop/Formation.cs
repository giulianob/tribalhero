#region

using System.Collections.Generic;

#endregion

namespace Game.Data.Troop
{
    public enum FormationType : byte
    {
        Normal = 1,
        Attack = 2,
        Defense = 3,
        Scout = 4,
        Garrison = 5,
        Structure = 6,
        InBattle = 7,
        Captured = 11,
        Wounded = 12,
        Killed = 13
    }

    public class Formation : Dictionary<ushort, ushort>
    {
        private readonly TroopStub parent;

        public Formation(FormationType type, TroopStub parent)
        {
            Type = type;
            this.parent = parent;
        }

        public FormationType Type { get; set; }

        public void FireUpdated()
        {
            parent.FireUpdated();
        }

        public new void Add(ushort type, ushort count)
        {
            ushort currentCount;
            if (TryGetValue(type, out currentCount))
                this[type] = (ushort)(currentCount + count);
            else
                this[type] = count;

            FireUpdated();
        }

        public ushort Remove(ushort type, ushort count)
        {
            ushort currentCount;
            if (TryGetValue(type, out currentCount))
            {
                if (currentCount <= count)
                {
                    Remove(type);
                    FireUpdated();
                    return currentCount;
                }

                var remaining = (ushort)(currentCount - count);
                this[type] = remaining;
                FireUpdated();
                return count;
            }
            return 0;
        }

        internal void Add(Formation formation)
        {
            foreach (var kvp in formation)
                Add(kvp.Key, kvp.Value);
            FireUpdated();
        }
    }
}
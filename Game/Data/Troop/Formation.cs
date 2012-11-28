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
        #region Events
        public delegate void UnitUpdated();
        public event UnitUpdated OnUnitUpdated = delegate { };
        #endregion

        public Formation(FormationType type)
        {
            Type = type;
        }

        public FormationType Type { get; set; }

        public new void Add(ushort type, ushort count)
        {
            ushort currentCount;
            if (TryGetValue(type, out currentCount))
                this[type] = (ushort)(currentCount + count);
            else
                this[type] = count;
            OnUnitUpdated();
        }

        public ushort Remove(ushort type, ushort count)
        {
            ushort currentCount;
            if (TryGetValue(type, out currentCount))
            {
                if (currentCount <= count)
                {
                    Remove(type);
                    OnUnitUpdated();
                    return currentCount;
                }

                var remaining = (ushort)(currentCount - count);
                this[type] = remaining;
                OnUnitUpdated();
                return count;
            }
            return 0;
        }

        internal void Add(Formation formation)
        {
            foreach (var kvp in formation)
                Add(kvp.Key, kvp.Value);
            OnUnitUpdated();
        }
    }
}
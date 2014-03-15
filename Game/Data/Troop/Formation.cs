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

        Garrison = 5,

        InBattle = 7
    }

    public class Formation : Dictionary<ushort, ushort>, IFormation
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
            {
                this[type] = (ushort)(currentCount + count);
            }
            else
            {
                this[type] = count;
            }
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

        public void Add(IFormation formation)
        {
            foreach (var kvp in formation)
            {
                Add(kvp.Key, kvp.Value);
            }

            OnUnitUpdated();
        }
    }
}
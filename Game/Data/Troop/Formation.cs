#region

using System.Collections.Generic;
using Game.Data.Troop;

#endregion

namespace Game.Fighting {
    public enum FormationType : byte {
        NORMAL = 1,
        ATTACK = 2,
        DEFENSE = 3,
        SCOUT = 4,
        GARRISON = 5,
        STRUCTURE = 6,
        IN_BATTLE = 7,
        CAPTURED = 11,
        WOUNDED = 12,
        KILLED = 13
    }

    public class Formation : Dictionary<ushort, ushort> {
        private readonly TroopStub parent;

        public FormationType Type { get; set; }

        public Formation(FormationType type, TroopStub parent) {
            Type = type;
            this.parent = parent;
        }

        public void FireUpdated() {
            parent.FireUpdated();
        }
        
        public new void Add(ushort type, ushort count) {
            ushort currentCount;
            if (TryGetValue(type, out currentCount))
                this[type] = (ushort) (currentCount + count);
            else
                this[type] = count;

            FireUpdated();
        }

        public ushort Remove(ushort type, ushort count) {
            ushort currentCount;
            if (TryGetValue(type, out currentCount)) {
                if (currentCount <= count) {
                    Remove(type);
                    FireUpdated();
                    return currentCount;
                }

                ushort remaining = (ushort) (currentCount - count);
                this[type] = remaining;
                FireUpdated();
                return count;
            }
            return 0;
        }

        internal void Add(Formation formation) {
            foreach (KeyValuePair<ushort, ushort> kvp in formation)
                Add(kvp.Key, kvp.Value);
            FireUpdated();
        }
    }
}
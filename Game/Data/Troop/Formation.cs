#region

using System.Collections.Generic;
using Game.Data;

#endregion

namespace Game.Fighting {
    public enum FormationType : byte {
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

    public class Formation : Dictionary<ushort, ushort> {
        private TroopStub parent = null;

        public Formation(TroopStub parent) {
            this.parent = parent;
        }

        public void FireUpdated() {
            parent.FireUpdated();
        }

        public void add(ushort type, ushort count) {
            ushort current_count;
            if (TryGetValue(type, out current_count))
                this[type] = (ushort) (current_count + count);
            else
                this[type] = count;

            FireUpdated();
        }

        public ushort remove(ushort type, ushort count) {
            ushort current_count;
            if (TryGetValue(type, out current_count)) {
                if (current_count <= count) {
                    Remove(type);
                    FireUpdated();
                    return current_count;
                } else {
                    ushort remaining = (ushort) (current_count - count);
                    this[type] = remaining;
                    FireUpdated();
                    return count;
                }
            }
            return 0;
        }

        internal void Add(Formation paperFormation) {
            foreach (KeyValuePair<ushort, ushort> kvp in paperFormation)
                add(kvp.Key, kvp.Value);
            FireUpdated();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace Game.Fighting {


    public class UnitVisibilityComparer : IComparer<Unit> {
        #region IComparer<Unit> Members

        public int Compare(Unit x, Unit y) {
            return x.stats.Stl.CompareTo(y.stats.Stl);
        }

        #endregion

    }
    [Serializable()]
    public class VisibleList : IEnumerable<Unit> {
        List<Unit> units;
        [NonSerialized()]
        UnitVisibilityComparer comparer = new UnitVisibilityComparer();
        public ushort Size {
            get { return (ushort)units.Count; }
        }
        public VisibleList(IEnumerable<Unit> units) {
            this.units = new List<Unit>(units);
            this.units.Sort(comparer);
        }

        public VisibleList(IEnumerable<BattleFormation> formations) {
            this.units = new List<Unit>();
            this.add(formations);
        }

        public IEnumerable<Unit> getList(ushort min_stealth, ushort max_stealth) {
            
            return units.FindAll(delegate(Unit unit) {
                return unit.stats.Stl >= min_stealth && unit.stats.Stl <= max_stealth;
            });
        }

        public void add(IEnumerable<BattleFormation> formations) {
            foreach (BattleFormation formation in formations) {
                units.AddRange(formation);
            }
            units.Sort(new UnitVisibilityComparer());
        }

        public void add(IEnumerable<Unit> units) {
            this.units.AddRange(units);
            this.units.Sort(comparer);
        }

        #region IEnumerable<Unit> Members

        public IEnumerator<Unit> GetEnumerator() {
            return units.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return units.GetEnumerator();
        }

        #endregion

        internal void remove(Unit target) {
            this.units.Remove(target);
        }
    }
}

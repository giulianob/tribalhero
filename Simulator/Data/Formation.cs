using System;
using System.Collections.Generic;
using System.Text;
using Simulator;

namespace Game.Fighting {
    [Serializable()]
    public class BattleFormation : IEnumerator<Unit>{
        List<Unit> units;
        int index = -1;

        public ushort Size {
            get { return (ushort)units.Count; }
        }
        public BattleFormation() {
            this.units = new List<Unit>();
        }
        public BattleFormation(IEnumerable<Unit> units) {
            this.units = new List<Unit>(units);
        }

        public void randomize() {
            Random random = new Random();
            units = new List<Unit>();
            int size = random.Next(5, 10);
            for (int i = 0; i < size; ++i) {
                Unit unit = new Unit();
                unit.randomize(random);
                units.Add(unit);
            }

        }
        public void print() {
            Console.Out.WriteLine("Formation");
            foreach (Unit unit in units) {
                unit.print();
            }
        }

        public void add(Unit unit) {
            units.Add(unit);
        }

        public void add(IEnumerable<Unit> lists) {
            units.AddRange(lists);
        }

        public bool remove(Unit unit) {
            int i = units.IndexOf(unit);
            if (i < 0) return false;
            if (i < this.index) --this.index;
            units.RemoveAt(i);
            return true;
        }
        public bool remove(int i) {
            if (i < 0) return false;
            if (i < this.index) --this.index;
            units.RemoveAt(i);
            return true;
        }

        public void shuffle() {
            Game.Util.Shuffle<Unit>.shuffleList(units);
        }

        public IEnumerable<Unit> DefaultEnumeration() {
            foreach (Unit unit in units) {
                yield return unit;
            }
        }

        #region IEnumerator<Unit> Members

        public Unit Current {
            get { return units[index]; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
            //throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region IEnumerator Members

        object System.Collections.IEnumerator.Current {
            get { return units[index]; }
        }

        public bool MoveNext() {
            if (index >= units.Count - 1) {
                return false;
            }
            ++index;
            return true;
        }

        public void Reset() {
            index = -1;
        }

        #endregion

        #region IEnumerable<Unit> Members

        public IEnumerator<Unit> GetEnumerator() {
            return this;
        }

        #endregion


    }
}

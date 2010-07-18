using System;
using System.Collections.Generic;
using System.Text;
using Game.Data;
using Simulator;

namespace Game.Fighting {

    [Serializable()]
    public class Troop : IEnumerable<KeyValuePair<FormationType, BattleFormation>> {
        [NonSerialized()]
        public uint id;

        public Dictionary<FormationType, BattleFormation> formations = new Dictionary<FormationType, BattleFormation>();

        public ushort Size {
            get {
                ushort count = 0;
                foreach (KeyValuePair<FormationType,BattleFormation> kvp in formations) {
                    count += kvp.Value.Size;
                } 
                return count;
            }
        }
        
        public Troop() {
       /*     Formation formation = new Formation();
            formation.randomize();
            formation.print();
            formations[FormationType.Normal] = formation;
            formation = new Formation();
            formation.randomize();
            formation.print();
            formations[FormationType.Attack] = formation;
            formation = new Formation();
            formation.randomize();
            formation.print();
            formations[FormationType.Defense] = formation;
            formation = new Formation();
            formation.randomize();
            formation.print();
            formations[FormationType.Scout] = formation;*/
          //  vlist = new VisibleList(formations.Values);
        }

        public bool add_formation(FormationType type) {
            if (formations.ContainsKey(type)) return false;
            formations.Add(type, new BattleFormation());
            return true;
        }

        public bool add_unit(FormationType formation_type, Unit unit) {
            BattleFormation formation;
            if (formations.TryGetValue(formation_type, out formation)) {
                formation.add(unit);
                return true;
            }
            return false;
        }

        public void add_units(FormationType formation_type, IEnumerable<Unit> units) {
            BattleFormation formation;
            if (formations.TryGetValue(formation_type, out formation)) {
                formation.add(units);
                return;
            }
            return;
        }

        public bool remove_unit(FormationType formation_type, Unit unit) {
            BattleFormation formation;
            if (formations.TryGetValue(formation_type, out formation)) {
                return formation.remove(unit);
            }
            return false;
        }

        public void print() {
            foreach (KeyValuePair<FormationType, BattleFormation> kvp in formations) {
                Console.Out.WriteLine("Formation type: " + Enum.GetName(typeof(FormationType), kvp.Key));
                foreach (Unit unit in kvp.Value) {
                    unit.print();
                }
            }
        }

        internal List<BattleFormation> getList(FormationType []formationType) {
            List<BattleFormation> list = new List<BattleFormation>();
            foreach (FormationType type in formationType) {
                if( formations.ContainsKey(type) ) 
                    list.Add(this.formations[type]);
            }
            return list;
        }

        internal bool remove(Unit target) {
            foreach (KeyValuePair<FormationType, BattleFormation> kvp in formations) {
                if (kvp.Value.remove(target)) return true;
            }
            return false;
        }

        #region IEnumerable<KeyValuePair<FormationType,Formation>> Members

        public IEnumerator<KeyValuePair<FormationType, BattleFormation>> GetEnumerator() {
            return formations.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return formations.GetEnumerator();
        }

        #endregion
    }

}

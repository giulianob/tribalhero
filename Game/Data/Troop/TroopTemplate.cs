using System;
using System.Collections.Generic;
using System.Text;
using Game.Database;
using Game.Fighting;
using Game.Setup;
using Game.Logic;
using Game.Battle;
using Game.Data.Stats;

namespace Game.Data.Troop {
    public class TroopTemplate : IPersistable {
        Dictionary<ushort, BattleStats> stats;

        public TroopTemplate(TroopStub stub) {
            stats = new Dictionary<ushort, BattleStats>();
            foreach (Formation formation in stub) {
                foreach(ushort type in formation.Keys) {
                    if (stats.ContainsKey(type)) continue;

                    BattleStats stat = BattleFormulas.LoadStats(type, stub.City.Template[type].Lvl, stub.City);
                    stats.Add(type, stat);
                }
            }      
        }

        public BattleStats this[ushort type] {
            get { return stats[type]; }
        }
    

        #region IPersistable Members

        public string DbTable {
            get { throw new NotImplementedException(); }
        }

        public DbColumn[] DbPrimaryKey {
            get { throw new NotImplementedException(); }
        }

        public DbDependency[] DbDependencies {
            get { throw new NotImplementedException(); }
        }

        public DbColumn[] DbColumns {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}

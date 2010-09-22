using System;
using System.Collections.Generic;
using System.Text;
using Game.Fighting;
using Game.Setup;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Simulator {
    [Serializable()]
    public class Unit : ICloneable {
        //[NonSerialized()]
        //public UnitStats stats;

        public int type;
        public byte lvl=1; 
        public ushort count;

        public UnitStats Stats {
            get { return UnitFactory.getUnitStats((ushort)type, lvl); }
        }
        internal void randomize(Random random) {
            type = (ushort)random.Next(1, 99);
            count = (ushort)random.Next(1, 20);
            // stats.randomize(random);
        }

        public void print() {
            Console.Out.WriteLine("type[{0}] name[{7}] cnt[{1}] stl[{2}] rng[{3}] atk[{4}] def[{5}]",
                                        type,
                                        count,
                                        Stats.stats.Stl,
                                        Stats.stats.Rng,
                                        Stats.stats.Atk,
                                        Stats.stats.Def,
                                        Stats.name);
        }

        #region ICloneable Members

        public object Clone() {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, this);
            ms.Position = 0;
            object obj = bf.Deserialize(ms);
            ms.Close();
            return obj;
        }

        #endregion
    }
}

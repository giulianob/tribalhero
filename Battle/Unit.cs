using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Game.Data;
using Game.Data.Stats;

namespace Game.Fighting {
 /*   [Serializable()]
    public class Stats {
        // for VisibleList 
        public ushort stealth;
        public byte vision;
        public byte range;

        public byte weapon_type;
        public byte armor_type;

        public byte atk;
        public byte def;

        public void randomize(Random random) {
            stealth = (ushort)random.Next(1, 100);
            vision = (byte)random.Next(1, 50);
            range = (byte)random.Next(1, 4);

            weapon_type = (byte)random.Next(1, 10);
            armor_type = (byte)random.Next(1, 10);

            atk = (byte)random.Next(1, 20);
            def = (byte)random.Next(1, 20);
        }
    }*/

    [Serializable()]
    public class Unit: ICloneable {
        [NonSerialized()]
        public uint id;
        [NonSerialized()]
        public FormationType formation_type;

        public ushort type;
        public string name;
        public ushort count;
        public byte lvl;

        public BattleStats stats = new BattleStats();


        internal void randomize(Random random) {
            type = (ushort)random.Next(1, 99);
            count = (ushort)random.Next(1, 20);
           // stats.randomize(random);
        }
        public void print() {
            Console.Out.WriteLine("type[{0}] name[{7}] cnt[{1}] stl[{2}] rng[{3}] atk[{4}] def[{5}]",
                                        type,
                                        count,
                                        stats.Stl,
                                        stats.Rng,
                                        stats.Atk,
                                        stats.Def,
                                        name);
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

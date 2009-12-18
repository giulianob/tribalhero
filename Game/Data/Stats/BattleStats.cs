using System;
using System.Collections.Generic;
using System.Text;
using Game.Setup;

namespace Game.Data {
    public enum ArmorType {
        Leather = 0,
        Metal = 1,
        Mount = 2,
        Wooden = 3,
        Stone = 4
    }

    public enum WeaponType {
        Sword = 0,
        Pike = 1,
        Bow = 2,
        FireBall = 3,
        StoneBall = 4
    }
    [Serializable()]
    public class BattleStats : ICloneable {

        #region Base Stats

        ushort groupSize;
        public ushort GroupSize {
            get { return groupSize; }            
        }

        WeaponType weapon;
        public WeaponType Weapon {
            get { return weapon; }
        }

        ArmorType armor;
        public ArmorType Armor {
            get { return armor; }
        }

        ushort maxHp = 0;
        public ushort MaxHp {
            get { return maxHp; }
            set { maxHp = value; }
        }

        byte atk = 0;
        public byte Atk {
            get { return atk; }
            set { atk = value; }
        }

        byte def = 0;
        public byte Def {
            get { return def; }
            set { def = value; }
        }

        byte rng = 0;
        public byte Rng {
            get { return rng; }
            set { rng = value; }
        }

        byte stl = 0;
        public byte Stl {
            get { return stl; }
            set { stl = value; }
        }

        byte spd = 0;
        public byte Spd {
            get { return spd; }
            set { spd = value; }
        }

        ushort reward;
        public ushort Reward {
            get { return reward; }
            set { reward = value; }
        }   

        #endregion

        #region Constructors
        public BattleStats() {
        }

        public BattleStats(WeaponType weapon, ArmorType armor, ushort maxHp, byte atk, byte def, byte range, byte stealth, byte speed, ushort groupSize, ushort reward) {
            this.weapon = weapon;
            this.armor = armor;
            this.maxHp = maxHp;
            this.atk = atk;
            this.def = def;
            this.rng = range;
            this.stl = stealth;
            this.spd = speed;
            this.groupSize = groupSize;
            this.reward = reward;
        }
        public BattleStats(BattleStats stats) {
            this.weapon = stats.weapon;
            this.armor = stats.armor;
            this.maxHp = stats.maxHp;
            this.atk = stats.atk;
            this.def = stats.def;
            this.rng = stats.rng;
            this.stl = stats.stl;
            this.spd = stats.spd;
            this.groupSize = stats.groupSize;
            this.reward = stats.reward;            
        }
        #endregion

        #region ICloneable Members

        public object Clone() {
            return this.MemberwiseClone();
        }

        #endregion
    }
}

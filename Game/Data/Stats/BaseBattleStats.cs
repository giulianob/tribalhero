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

    public class BaseBattleStats {
        #region Base Stats

        private ushort type;

        public ushort Type {
            get { return type; }
        }

        private byte lvl;

        public byte Lvl {
            get { return lvl; }
        }

        private ushort groupSize;

        public ushort GroupSize {
            get { return groupSize; }
        }

        private WeaponType weapon;

        public WeaponType Weapon {
            get { return weapon; }
        }

        private ArmorType armor;

        public ArmorType Armor {
            get { return armor; }
        }

        private ushort maxHp = 0;

        public ushort MaxHp {
            get { return maxHp; }
        }

        private byte atk = 0;

        public byte Atk {
            get { return atk; }
        }

        private byte def = 0;

        public byte Def {
            get { return def; }
        }

        private byte rng = 0;

        public byte Rng {
            get { return rng; }
        }

        private byte stl = 0;

        public byte Stl {
            get { return stl; }
        }

        private byte spd = 0;

        public byte Spd {
            get { return spd; }
        }

        private ushort reward;

        public ushort Reward {
            get { return reward; }
        }

        #endregion

        #region Constructors

        public BaseBattleStats(ushort type, byte lvl, WeaponType weapon, ArmorType armor, ushort maxHp, byte atk,
                               byte def, byte range, byte stealth, byte speed, ushort groupSize, ushort reward) {
            this.type = type;
            this.lvl = lvl;
            this.weapon = weapon;
            this.armor = armor;
            this.maxHp = maxHp;
            this.atk = atk;
            this.def = def;
            rng = range;
            stl = stealth;
            spd = speed;
            this.groupSize = groupSize;
            this.reward = reward;
        }

        #endregion
    }
}
namespace Game.Data {
    public enum ArmorType {
        GROUND = 0,
        MOUNT = 1,
        MACHINE = 2,
        BUILDING = 3
    }

    public enum ArmorClass {
        LEATHER = 0,
        METAL = 1,
        WOODEN = 2,
        STONE = 3
    }

    public enum WeaponType {
        SWORD = 0,
        PIKE = 1,
        BOW = 2,
        BALL = 3
    }

    public enum WeaponClass {
        BASIC = 0,
        ELEMENTAL = 1
    }

    public class BaseBattleStats {
        #region Base Stats

        public ushort Type { get; private set; }

        public byte Lvl { get; private set; }

        public ushort GroupSize { get; private set; }

        public WeaponType Weapon { get; private set; }

        public WeaponClass WeaponClass { get; private set; }

        public ArmorType Armor { get; private set; }

        public ArmorClass ArmorClass { get; private set; }

        public ushort MaxHp { get; private set; }

        public ushort Atk { get; private set; }

        public ushort Def { get; private set; }

        public byte Rng { get; private set; }

        public byte Stl { get; private set; }

        public byte Spd { get; private set; }

        public ushort Reward { get; private set; }

        public byte Carry { get; private set; }

        #endregion

        #region Constructors

        public BaseBattleStats(ushort type, byte lvl, WeaponType weapon, WeaponClass wpnClass, ArmorType armor, ArmorClass armrClass, ushort maxHp, ushort atk,
                               ushort def, byte range, byte stealth, byte speed, ushort groupSize, ushort reward, byte carry) {
            Type = type;
            Lvl = lvl;
            Weapon = weapon;
            WeaponClass = wpnClass;
            Armor = armor;
            ArmorClass = ArmorClass;
            MaxHp = maxHp;
            Atk = atk;
            Def = def;
            Rng = range;
            Stl = stealth;
            Spd = speed;
            GroupSize = groupSize;
            Reward = reward;
            Carry = carry;
        }

        #endregion
    }
}
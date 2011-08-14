namespace Game.Data
{
    public enum ArmorType
    {
        Ground = 0,
        Mount = 1,
        Machine = 2,
        Building1 = 4,
        Building2 = 5,
        Building3 = 6,
    }

    public enum ArmorClass
    {
        Leather = 0,
        Metal = 1,
        Wooden = 2,
        Stone = 3
    }

    public enum WeaponType
    {
        Sword = 0,
        Pike = 1,
        Bow = 2,
        Ball = 3,
        Barricade = 4
    }

    public enum WeaponClass
    {
        Basic = 0,
        Elemental = 1
    }

    public class BaseBattleStats
    {
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

        public byte Splash { get; private set; }

        public ushort Def { get; private set; }

        public byte Rng { get; private set; }

        public byte Stl { get; private set; }

        public byte Spd { get; private set; }

        public ushort Carry { get; private set; }

        #endregion

        #region Constructors

        public BaseBattleStats(ushort type,
                               byte lvl,
                               WeaponType weapon,
                               WeaponClass wpnClass,
                               ArmorType armor,
                               ArmorClass armrClass,
                               ushort maxHp,
                               ushort atk,
                               byte splash,
                               ushort def,
                               byte range,
                               byte stealth,
                               byte speed,
                               ushort groupSize,
                               ushort carry)
        {
            Type = type;
            Lvl = lvl;
            Weapon = weapon;
            WeaponClass = wpnClass;
            Armor = armor;
            ArmorClass = armrClass;
            MaxHp = maxHp;
            Atk = atk;
            Splash = splash;
            Def = def;
            Rng = range;
            Stl = stealth;
            Spd = speed;
            GroupSize = groupSize;
            Carry = carry;
        }

        #endregion
    }
}
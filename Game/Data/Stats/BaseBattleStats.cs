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

        Gate = 7
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

        Barricade = 4,

        Tower = 5,

        Cannon = 6,
    }

    public enum WeaponClass
    {
        Basic = 0,

        Elemental = 1
    }

    public class BaseBattleStats : IBaseBattleStats
    {
        #region Base Stats

        public virtual ushort Type { get; private set; }

        public virtual byte Lvl { get; private set; }

        public virtual ushort GroupSize { get; private set; }

        public virtual WeaponType Weapon { get; private set; }

        public virtual WeaponClass WeaponClass { get; private set; }

        public virtual ArmorType Armor { get; private set; }

        public virtual ArmorClass ArmorClass { get; private set; }

        public virtual decimal MaxHp { get; private set; }

        public virtual decimal Atk { get; private set; }

        public virtual byte Splash { get; private set; }

        public virtual byte Rng { get; private set; }

        public virtual byte Stl { get; private set; }

        public virtual byte Spd { get; private set; }

        public virtual ushort Carry { get; private set; }

        public virtual decimal NormalizedCost { get; private set; }

        #endregion

        #region Constructors

        public BaseBattleStats()
        {
        }

        public BaseBattleStats(ushort type,
                               byte lvl,
                               WeaponType weapon,
                               WeaponClass wpnClass,
                               ArmorType armor,
                               ArmorClass armrClass,
                               decimal maxHp,
                               decimal atk,
                               byte splash,
                               byte range,
                               byte stealth,
                               byte speed,
                               ushort groupSize,
                               ushort carry,
                               decimal normalizedCost)
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
            Rng = range;
            Stl = stealth;
            Spd = speed;
            GroupSize = groupSize;
            Carry = carry;
            NormalizedCost = normalizedCost;
        }

        #endregion
    }
}
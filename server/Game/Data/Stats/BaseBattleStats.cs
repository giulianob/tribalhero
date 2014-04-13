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

        public virtual decimal Attack { get; private set; }

        public virtual byte Splash { get; private set; }

        public virtual byte Range { get; private set; }

        public virtual byte Stealth { get; private set; }

        public virtual byte Speed { get; private set; }

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
                               WeaponClass weaponClass,
                               ArmorType armor,
                               ArmorClass armorClass,
                               decimal maxHp,
                               decimal attack,
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
            WeaponClass = weaponClass;
            Armor = armor;
            ArmorClass = armorClass;
            MaxHp = maxHp;
            Attack = attack;
            Splash = splash;
            Range = range;
            Stealth = stealth;
            Speed = speed;
            GroupSize = groupSize;
            Carry = carry;
            NormalizedCost = normalizedCost;
        }

        public BaseBattleStats(IBaseBattleStats copy)
                : this(copy.Type,
                       copy.Lvl,
                       copy.Weapon,
                       copy.WeaponClass,
                       copy.Armor,
                       copy.ArmorClass,
                       copy.MaxHp,
                       copy.Attack,
                       copy.Splash,
                       copy.Range,
                       copy.Stealth,
                       copy.Speed,
                       copy.GroupSize,
                       copy.Carry,
                       copy.NormalizedCost)
        {
        }

        #endregion
    }
}
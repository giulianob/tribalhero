namespace Game.Data
{
    public interface IBaseBattleStats
    {
        ushort Type { get; }

        byte Lvl { get; }

        ushort GroupSize { get; }

        WeaponType Weapon { get; }

        WeaponClass WeaponClass { get; }

        ArmorType Armor { get; }

        ArmorClass ArmorClass { get; }

        decimal MaxHp { get; }

        decimal Attack { get; }

        byte Splash { get; }

        byte Range { get; }

        byte Stealth { get; }

        byte Speed { get; }

        ushort Carry { get; }

        decimal NormalizedCost { get; }
    }
}
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

        decimal Atk { get; }

        byte Splash { get; }

        byte Rng { get; }

        byte Stl { get; }

        byte Spd { get; }

        ushort Carry { get; }

        decimal NormalizedCost { get; }
    }
}
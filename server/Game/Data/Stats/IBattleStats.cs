namespace Game.Data.Stats
{
    public interface IBattleStats
    {
        decimal MaxHp { get; set; }

        decimal Atk { get; set; }

        byte Splash { get; set; }

        byte Rng { get; set; }

        byte Stl { get; set; }

        byte Spd { get; set; }

        ushort Carry { get; set; }

        decimal NormalizedCost { get; set; }

        IBaseBattleStats Base { get; }

        event BaseStats.OnStatsUpdate StatsUpdate;
    }
}
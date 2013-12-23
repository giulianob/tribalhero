namespace Game.Data.Stats
{
    public interface IStructureStats
    {
        decimal Hp { get; set; }

        ushort Labor { get; set; }

        IStructureBaseStats Base { get; }

        event BaseStats.OnStatsUpdate StatsUpdate;
    }
}
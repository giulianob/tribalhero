namespace Game.Data.Stats
{
    public interface IStructureBaseStats
    {
        string Name { get; }

        string SpriteClass { get; }

        ushort Type { get; }

        byte Lvl { get; }

        Resource Cost { get; }

        byte Size { get; }

        int BuildTime { get; }

        int WorkerId { get; }

        ushort MaxLabor { get; }

        byte Radius { get; }

        IBaseBattleStats Battle { get; }

        int StructureHash { get; }
    }
}
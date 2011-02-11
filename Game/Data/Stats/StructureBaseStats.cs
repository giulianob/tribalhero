#region

using Game.Setup;

#endregion

namespace Game.Data.Stats
{
    public class StructureBaseStats
    {
        public StructureBaseStats(string name,
                                  string spriteClass,
                                  ushort type,
                                  byte lvl,
                                  byte radius,
                                  Resource cost,
                                  BaseBattleStats baseBattleStats,
                                  byte maxLabor,
                                  int buildTime,
                                  int workerId,
                                  ClassId baseClass)
        {
            Name = name;
            SpriteClass = spriteClass;
            Radius = radius;
            Type = type;
            Lvl = lvl;
            Cost = cost;
            Battle = baseBattleStats;
            MaxLabor = maxLabor;
            BuildTime = buildTime;
            BaseClass = baseClass;
            WorkerId = workerId;
        }

        public string Name { get; private set; }

        public string SpriteClass { get; private set; }

        public ushort Type { get; private set; }

        public byte Lvl { get; private set; }

        public Resource Cost { get; private set; }

        public int BuildTime { get; private set; }

        public ClassId BaseClass { get; private set; }

        public int WorkerId { get; private set; }

        public byte MaxLabor { get; private set; }

        public byte Radius { get; private set; }

        public BaseBattleStats Battle { get; private set; }

        public int StructureHash
        {
            get
            {
                return Type*100 + Lvl;
            }
        }
    }
}
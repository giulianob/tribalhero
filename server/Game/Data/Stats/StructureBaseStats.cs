#region



#endregion

namespace Game.Data.Stats
{
    public class StructureBaseStats : IStructureBaseStats
    {
        public StructureBaseStats(string name,
                                  string spriteClass,
                                  ushort type,
                                  byte lvl,
                                  byte radius,
                                  Resource cost,
                                  BaseBattleStats baseBattleStats,
                                  ushort maxLabor,
                                  int buildTime,
                                  int workerId,
                                  byte size)
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
            WorkerId = workerId;
            Size = size;
        }

        public StructureBaseStats(StructureBaseStats copy) :
                this(copy.Name,
                     copy.SpriteClass,
                     copy.Type,
                     copy.Lvl,
                     copy.Radius,
                     new Resource(copy.Cost),
                     new BaseBattleStats(copy.Battle),
                     copy.MaxLabor,
                     copy.BuildTime,
                     copy.WorkerId,
                     copy.Size)
        {
        }

        public virtual string Name { get; private set; }

        public virtual string SpriteClass { get; private set; }

        public virtual ushort Type { get; private set; }

        public virtual byte Lvl { get; private set; }

        public virtual Resource Cost { get; private set; }

        public virtual int BuildTime { get; private set; }

        public virtual int WorkerId { get; private set; }

        public virtual byte Size { get; set; }

        public virtual ushort MaxLabor { get; private set; }

        public virtual byte Radius { get; private set; }

        public virtual IBaseBattleStats Battle { get; private set; }

        public virtual int StructureHash
        {
            get
            {
                return Type * 100 + Lvl;
            }
        }
    }
}
using System;

namespace Game.Data.Stats
{
    public class StructureStats : BaseStats, IStructureStats
    {
        private decimal hp;

        private ushort labor;

        public StructureStats()
        {
        }

        public StructureStats(IStructureBaseStats baseStats)
        {
            Base = baseStats;

            hp = baseStats.Battle.MaxHp;
        }

        public virtual decimal Hp
        {
            get
            {
                return hp;
            }
            set
            {
                hp = Math.Min(value, Base.Battle.MaxHp);
                FireStatsUpdate();
            }
        }

        public virtual ushort Labor
        {
            get
            {
                return labor;
            }
            set
            {
                labor = value;
                FireStatsUpdate();
            }
        }

        public virtual IStructureBaseStats Base { get; private set; }
    }
}
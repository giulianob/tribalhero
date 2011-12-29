using System;
namespace Game.Data.Stats
{
    public class StructureStats : BaseStats
    {
        private ushort hp;

        private ushort labor;

        public StructureStats(StructureBaseStats baseStats)
        {
            Base = baseStats;

            hp = baseStats.Battle.MaxHp;
        }

        public virtual ushort Hp
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

        public virtual StructureBaseStats Base { get; private set; }
    }
}
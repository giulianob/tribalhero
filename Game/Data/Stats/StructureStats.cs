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

        public ushort Hp
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

        public ushort Labor
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

        public StructureBaseStats Base { get; private set; }
    }
}
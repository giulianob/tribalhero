namespace Game.Data.Stats
{
    public class BattleStats : BaseStats
    {
        #region Base Stats

        public virtual decimal MaxHp { get; set; }

        public virtual decimal Atk { get; set; }

        public virtual byte Splash { get; set; }

        public virtual byte Rng { get; set; }

        public virtual byte Stl { get; set; }

        public virtual byte Spd { get; set; }

        public virtual ushort Carry { get; set; }

        public virtual decimal NormalizedCost { get; set; }

        public virtual BaseBattleStats Base { get; private set; }

        #endregion

        #region Constructors

        public BattleStats()
        {
        }

        public BattleStats(BaseBattleStats baseStats)
        {
            Base = baseStats;
            MaxHp = baseStats.MaxHp;
            Atk = baseStats.Atk;
            Splash = baseStats.Splash;
            Rng = baseStats.Rng;
            Stl = baseStats.Stl;
            Spd = baseStats.Spd;
            Carry = baseStats.Carry;
            NormalizedCost = baseStats.NormalizedCost;
        }

        #endregion
    }
}
namespace Game.Data.Stats
{
    public class BattleStats : BaseStats, IBattleStats
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

        public virtual IBaseBattleStats Base { get; private set; }

        #endregion

        #region Constructors

        public BattleStats()
        {
        }

        public BattleStats(IBaseBattleStats baseStats)
        {
            Base = baseStats;
            MaxHp = baseStats.MaxHp;
            Atk = baseStats.Attack;
            Splash = baseStats.Splash;
            Rng = baseStats.Range;
            Stl = baseStats.Stealth;
            Spd = baseStats.Speed;
            Carry = baseStats.Carry;
            NormalizedCost = baseStats.NormalizedCost;
        }

        #endregion
    }
}
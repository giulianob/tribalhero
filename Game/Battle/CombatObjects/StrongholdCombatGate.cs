using Game.Data;
using Game.Data.Stronghold;
using Game.Setup;

namespace Game.Battle.CombatObjects
{
    public class StrongholdCombatGate : StrongholdCombatStructure
    {
        public StrongholdCombatGate(uint id,
                                    uint battleId,
                                    ushort type,
                                    byte lvl,
                                    decimal hp,
                                    IStronghold stronghold,
                                    IStructureCsvFactory structureCsvFactory,
                                    IBattleFormulas battleFormulas)
                : base(id, battleId, type, lvl, hp, stronghold, structureCsvFactory, battleFormulas)
        {
        }

        public override void TakeDamage(decimal dmg, out Resource returning, out int attackPoints)
        {
            base.TakeDamage(dmg, out returning, out attackPoints);
            Stronghold.BeginUpdate();
            Stronghold.Gate = hp;
            Stronghold.EndUpdate();
        }

        public override void CalcActualDmgToBeTaken(ICombatList attackers,
                                                    ICombatList defenders,
                                                    IBattleRandom random,
                                                    decimal baseDmg,
                                                    int attackIndex,
                                                    out decimal actualDmg)
        {
            actualDmg = baseDmg / 10;
        }
    }
}
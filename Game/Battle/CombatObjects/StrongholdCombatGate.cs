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
                                    StructureFactory structureFactory,
                                    BattleFormulas battleFormulas)
                : base(id, battleId, type, lvl, hp, stronghold, structureFactory, battleFormulas)
        {
        }

        public override void TakeDamage(decimal dmg, out Resource returning, out int attackPoints)
        {
            base.TakeDamage(dmg, out returning, out attackPoints);
            Stronghold.Gate = hp;
        }

        public override void CalcActualDmgToBeTaken(ICombatList attackers,
                                                    ICombatList defenders,
                                                    decimal baseDmg,
                                                    int attackIndex,
                                                    out decimal actualDmg)
        {
            baseDmg /= 10;
            base.CalcActualDmgToBeTaken(attackers, defenders, baseDmg, attackIndex, out actualDmg);
        }
    }
}
using Game.Data;
using Game.Data.Stronghold;
using Game.Data.Troop;

namespace Game.Battle.CombatObjects
{
    public interface ICombatUnitFactory
    {
        CombatStructure CreateStructureCombatUnit(IBattleManager battleManager, IStructure structure);

        AttackCombatUnit[] CreateAttackCombatUnit(IBattleManager battleManager, ITroopObject troop, FormationType formation, ushort type, ushort count);

        DefenseCombatUnit[] CreateDefenseCombatUnit(IBattleManager battleManager, ITroopStub stub, FormationType formation, ushort type, ushort count);

        StrongholdCombatUnit CreateStrongholdCombatUnit(IBattleManager battleManager, IStronghold stronghold, ushort type, byte level, ushort count);

        StrongholdCombatStructure CreateStrongholdGateStructure(IBattleManager battleManager, IStronghold stronghold, decimal hp);
    }
}
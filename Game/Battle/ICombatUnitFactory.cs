using Game.Data;
using Game.Data.Troop;

namespace Game.Battle
{
    public interface ICombatUnitFactory
    {
        CombatStructure CreateStructureCombatUnit(IBattleManager battleManager, Structure structure);
        AttackCombatUnit[] CreateAttackCombatUnit(IBattleManager owner, TroopObject troop, FormationType formation, ushort type, ushort count);
        DefenseCombatUnit[] CreateDefenseCombatUnit(IBattleManager owner, TroopStub stub, FormationType formation, ushort type, ushort count);
    }
}
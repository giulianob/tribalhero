using Game.Data;
using Game.Data.Troop;

namespace Game.Battle
{
    public interface ICombatUnitFactory
    {
        CombatStructure CreateStructureCombatUnit(IBattleManager battleManager, IStructure structure);
        AttackCombatUnit[] CreateAttackCombatUnit(IBattleManager owner, ITroopObject troop, FormationType formation, ushort type, ushort count);
        DefenseCombatUnit[] CreateDefenseCombatUnit(IBattleManager owner, ITroopStub stub, FormationType formation, ushort type, ushort count);
    }
}
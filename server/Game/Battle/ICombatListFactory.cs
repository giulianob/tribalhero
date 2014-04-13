namespace Game.Battle
{
    public interface ICombatListFactory
    {
        ICombatList GetDefenderCombatList();
        
        ICombatList GetAttackerCombatList();
    }
}
using Game.Data;

namespace Game.Battle.CombatObjects
{
    public interface ICombatStructure : ICombatObject
    {
        IStructure Structure { get; }
    }
}
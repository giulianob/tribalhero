#region

using Game.Data;

#endregion

namespace Game.Logic.Conditons
{
    public interface IStructureCondition
    {
        bool Check(IStructure obj);
    }
}
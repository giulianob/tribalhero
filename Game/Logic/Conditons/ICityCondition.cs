#region

using Game.Data;

#endregion

namespace Game.Logic.Conditons
{
    public interface ICityCondition
    {
        bool Check(ICity obj);
    }
}
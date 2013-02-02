using Game.Logic;
using Persistance;

namespace Game.Data.Settlement
{
    public interface ISettlement : ICityRegionObject, ISimpleGameObject, IPersistableObject, IHasLevel, ICanDo, ILocation
    {
    }
}
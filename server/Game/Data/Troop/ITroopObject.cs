using Game.Data.Stats;
using Persistance;

namespace Game.Data.Troop
{
    public interface ITroopObject : IPersistableObject, IGameObject, IMiniMapRegionObject
    {
        ITroopStub Stub { get; set; }

        uint TargetX { get; set; }

        uint TargetY { get; set; }

        TroopStats Stats { get; set; }

        string Theme { get; set; }
    }
}
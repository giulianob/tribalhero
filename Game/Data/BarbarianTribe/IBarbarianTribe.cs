using System;
using Game.Battle;
using Game.Logic;
using Persistance;

namespace Game.Data.BarbarianTribe
{
    public interface IBarbarianTribe : ICityRegionObject, ISimpleGameObject, IPersistableObject, IHasLevel, ICanDo, ILocation
    {
        uint Id { get; }

        IBattleManager Battle { get; set; }

        IActionWorker Worker { get; }

        Resource Resource { get; }

        DateTime Created { get; set; }

        DateTime LastAttacked { get; set; }

        byte CampRemains { get; }
    }
}
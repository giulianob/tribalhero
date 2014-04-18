using System;
using Game.Battle;
using Game.Logic;
using Persistance;

namespace Game.Data.BarbarianTribe
{
    public interface IBarbarianTribe : IMiniMapRegionObject, ISimpleGameObject, IPersistableObject, IHasLevel, ICanDo, ILocation
    {
        IBattleManager Battle { get; set; }

        IActionWorker Worker { get; }

        Resource Resource { get; }

        DateTime Created { get; set; }

        DateTime LastAttacked { get; set; }

        byte CampRemains { set; get; }        

        event EventHandler<EventArgs> CampRemainsChanged;
    }
}
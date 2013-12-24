using System;
using System.Collections.Generic;
using Game.Data;
using Game.Data.Events;

namespace Game.Logic
{
    public interface IReferenceManager : IEnumerable<ReferenceStub>
    {
        event EventHandler<ActionReferenceArgs> ReferenceAdded;

        event EventHandler<ActionReferenceArgs> ReferenceRemoved;

        ushort Count { get; }

        void DbLoaderAdd(ReferenceStub referenceObject);

        void Add(IGameObject referenceObject, PassiveAction action);

        void Add(IGameObject referenceObject, ActiveAction action);

        void Remove(IGameObject referenceObject, GameAction action);

        void Remove(IGameObject referenceObject);
    }
}
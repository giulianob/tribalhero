using System.Collections.Generic;
using Game.Logic;
using Persistance;

namespace Game.Data
{
    public interface ITechnologyManager : IHasEffect, IEnumerable<Technology>, IPersistableList
    {
        EffectLocation OwnerLocation { get; }

        uint OwnerId { get; }

        int TechnologyCount { get; }

        int OwnedTechnologyCount { get; }

        ITechnologyManager Parent { get; set; }

        uint Id { get; set; }

        bool Add(Technology tech);

        bool Add(Technology tech, bool notify);

        bool Upgrade(Technology tech);

        void Clear();

        List<Effect> GetEffects(EffectCode effectCode, EffectInheritance inherit = EffectInheritance.All);

        void BeginUpdate();

        void EndUpdate();

        event TechnologyManager.TechnologyUpdatedCallback TechnologyAdded;

        event TechnologyManager.TechnologyUpdatedCallback TechnologyRemoved;

        event TechnologyManager.TechnologyUpdatedCallback TechnologyUpgraded;

        event TechnologyManager.TechnologyClearedCallback TechnologyCleared;

        void Print();

        bool TryGetTechnology(uint techType, out Technology technology);

        void AddChildCopy(Technology tech);

        void RemoveChildCopy(Technology tech, bool notify);

        bool Remove(uint techType);
    }
}
using Game.Data.Stats;
using Game.Logic;
using Persistance;

namespace Game.Data
{
    public interface IStructure : IHasLevel, IPersistableObject, IGameObject
    {
        object this[string name] { get; set; }

        TechnologyManager Technologies { get; }

        StructureProperties Properties { get; set; }

        bool IsMainBuilding { get; }

        StructureStats Stats { get; set; }
    }
}
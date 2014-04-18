using Game.Data.Stats;
using Persistance;

namespace Game.Data
{
    public interface IStructure : IHasLevel, IPersistableObject, IGameObject
    {
        object this[string name] { get; set; }

        ITechnologyManager Technologies { get; }

        StructureProperties Properties { get; }

        bool IsMainBuilding { get; }

        IStructureStats Stats { get; set; }

        string Theme { get; set; }
    }
}
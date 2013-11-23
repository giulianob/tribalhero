using System.Collections.Generic;
using Game.Data;

namespace Game.Setup
{
    public interface IObjectTypeFactory
    {
        void Init(string filename);

        bool IsStructureType(string type, IStructure structure);

        bool IsObjectType(string type, ushort objectType);

        uint[] GetTypes(string type);

        bool IsTileType(string type, uint tileType);

        bool HasTileType(string type, IEnumerable<ushort> tileTypes);

        bool IsAllTileType(string type, IEnumerable<ushort> tileTypes);
    }
}
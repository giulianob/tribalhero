using System.Collections.Generic;
using Game.Data;
using Game.Data.Stats;

namespace Game.Setup
{
    public interface IStructureCsvFactory
    {
        void Init(string filename);

        Resource GetCost(ushort type, int lvl);

        int GetTime(ushort type, byte lvl);

        void GetUpgradedStructure(IStructure structure, ushort type, byte lvl);

        int GetActionWorkerType(IStructure structure);

        string GetName(ushort type, byte lvl);

        IStructureBaseStats GetBaseStats(ushort type, byte lvl);

        IEnumerable<IStructureBaseStats> AllStructures();
    }
}
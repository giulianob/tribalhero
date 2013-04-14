using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data;
using Game.Logic.Triggers.Events;

namespace Game.Logic.Triggers
{
    public interface ICityEventFactory
    {
        StructureInitEvent CreateStructureInitEvent(IStructure structure, ushort type);
        StructureUpgradeEvent CreateStructureUpgradeEvent(IStructure structure, ushort type, byte level);
        StructureUpgradeEvent CreateStructureDowngradeEvent(IStructure structure, ushort type, byte level);
        StructureUpgradeEvent CreateStructureConvertEvent(IStructure structure, ushort type, byte level);

        TechnologyDeleteEvent CreateTechnologyDeleteEvent(IStructure structure, uint type, byte level);
        TechnologyUpgradeEvent CreateTechnologyUpgradeEvent(IStructure structure, uint type, byte level);
    }
}

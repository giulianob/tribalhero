using Game.Data;
using Game.Logic.Triggers.Events;

namespace Game.Logic.Triggers
{
    public interface ICityEventFactory
    {
        StructureUpgradeEvent CreateStructureUpgradeEvent(IStructure structure, int type, byte level);
        
        StructureDowngradeEvent CreateStructureDowngradeEvent(IStructure structure, int type, byte level);
        
        StructureConvertEvent CreateStructureConvertEvent(IStructure structure, int type, byte level);
        
        TechnologyDeleteEvent CreateTechnologyDeleteEvent(IStructure structure, uint type, byte level);
        
        TechnologyUpgradeEvent CreateTechnologyUpgradeEvent(IStructure structure, uint type, byte level);
    }
}

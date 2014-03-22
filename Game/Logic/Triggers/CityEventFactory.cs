using Game.Data;
using Game.Logic.Triggers.Events;

namespace Game.Logic.Triggers
{
    public class CityEventFactory : ICityEventFactory
    {
        public StructureUpgradeEvent CreateStructureUpgradeEvent(IStructure structure, int type, byte level)
        {
            return new StructureUpgradeEvent(structure, type, level);
        }

        public StructureDowngradeEvent CreateStructureDowngradeEvent(IStructure structure, int type, byte level)
        {
            return new StructureDowngradeEvent(structure, type, level);
        }

        public StructureConvertEvent CreateStructureConvertEvent(IStructure structure, int type, byte level)
        {
            return new StructureConvertEvent(structure, type, level);
        }

        public TechnologyDeleteEvent CreateTechnologyDeleteEvent(IStructure structure, uint type, byte level)
        {
            return new TechnologyDeleteEvent(structure, type, level);
        }

        public TechnologyUpgradeEvent CreateTechnologyUpgradeEvent(IStructure structure, uint type, byte level)
        {
            return new TechnologyUpgradeEvent(structure, type, level);
        }
    }
}
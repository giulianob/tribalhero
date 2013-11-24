#region

using Game.Data;
using Game.Logic.Formulas;
using Game.Logic.Triggers;
using Game.Logic.Triggers.Events;

#endregion

namespace Game.Logic.Procedures
{
    public class CallbackProcedure
    {
        private readonly ICityTriggerManager cityTriggerManager;
        private readonly ICityEventFactory cityEventFactory;

        public CallbackProcedure(ICityTriggerManager cityTriggerManager, ICityEventFactory cityEventFactory)
        {
            this.cityTriggerManager = cityTriggerManager;
            this.cityEventFactory = cityEventFactory;
        }

        public virtual void OnStructureDowngrade(IStructure structure)
        {
            cityTriggerManager.Process(cityEventFactory.CreateStructureDowngradeEvent(structure, structure.Type, structure.Lvl));
        }

        public virtual void OnTechnologyUpgrade(IStructure structure, TechnologyBase technologyBase)
        {
            cityTriggerManager.Process(cityEventFactory.CreateTechnologyUpgradeEvent(structure, technologyBase.Techtype, technologyBase.Level));
        }

        public virtual void OnTechnologyDelete(IStructure structure, TechnologyBase technologyBase)
        {
            cityTriggerManager.Process(cityEventFactory.CreateTechnologyDeleteEvent(structure, technologyBase.Techtype, technologyBase.Level));
        }
    }
}
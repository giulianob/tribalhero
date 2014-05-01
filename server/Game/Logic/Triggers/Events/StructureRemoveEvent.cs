using Game.Data;
using Game.Logic.Conditons;

namespace Game.Logic.Triggers.Events
{
    public class StructureRemoveEvent : ICityEvent 
    {
        private readonly IStructure structure;
        private readonly int type;

        public StructureRemoveEvent(IStructure structure, int type)
        {
            this.structure = structure;
            this.type = type;
        }

        public IGameObject GameObject
        {
            get
            {
                return structure;
            }
        }

        public dynamic Parameters
        {
            get
            {
                return new {type};
            }
        }
    }
}

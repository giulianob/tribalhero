#region

using System.Collections.Generic;
using Game.Data;

#endregion

namespace Game.Logic.Requirements.LayoutRequirements
{
    public abstract class LayoutRequirement : ILayoutRequirement
    {
        protected List<Requirement> Requirements = new List<Requirement>();

        public abstract bool Validate(IStructure builder, ushort type, uint x, uint y, byte size);

        public void Add(Requirement req)
        {
            Requirements.Add(req);
        }
    }
}
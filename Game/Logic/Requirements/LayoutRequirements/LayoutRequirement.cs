#region

using System.Collections.Generic;
using Game.Data;

#endregion

namespace Game.Logic.Requirements.LayoutRequirements
{
    enum LayoutComparison : byte
    {
        NotContains = 0,

        Contains = 1
    }

    public abstract class LayoutRequirement
    {
        protected List<Requirement> requirements = new List<Requirement>();

        public abstract bool Validate(IStructure builder, ushort type, uint x, uint y);

        public void Add(Requirement req)
        {
            requirements.Add(req);
        }
    }
}
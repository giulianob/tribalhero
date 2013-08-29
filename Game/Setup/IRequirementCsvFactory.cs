using Game.Logic.Requirements.LayoutRequirements;

namespace Game.Setup
{
    public interface IRequirementCsvFactory
    {
        void Init(string filename);

        ILayoutRequirement GetLayoutRequirement(ushort type, byte lvl);
    }
}
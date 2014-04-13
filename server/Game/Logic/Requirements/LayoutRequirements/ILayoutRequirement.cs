using Game.Data;

namespace Game.Logic.Requirements.LayoutRequirements
{
    public interface ILayoutRequirement
    {
        bool Validate(IStructure builder, ushort type, uint x, uint y, byte size);

        void Add(Requirement req);
    }
}
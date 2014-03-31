using Game.Data.Troop;

namespace Game.Data
{
    public interface IGameObjectFactory
    {
        IStructure CreateStructure(uint cityId, uint structureId, ushort type, byte level, uint x, uint y, string theme);

        ITroopObject CreateTroopObject(uint id, ITroopStub stub, uint x, uint y);
    }
}
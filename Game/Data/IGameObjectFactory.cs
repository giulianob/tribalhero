namespace Game.Data
{
    public interface IGameObjectFactory
    {
        IStructure CreateStructure(uint cityId, uint structureId, ushort type, byte level);
    }
}
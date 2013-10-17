namespace Game.Data
{
    public interface ISimpleStubGeneratorFactory
    {
        SimpleStubGenerator CreateSimpleStubGenerator(double[][] ratio, ushort[] type);
    }
}
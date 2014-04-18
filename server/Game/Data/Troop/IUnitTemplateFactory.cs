namespace Game.Data.Troop
{
    public interface IUnitTemplateFactory
    {
        IUnitTemplate CreateUnitTemplate(uint cityId);
    }
}
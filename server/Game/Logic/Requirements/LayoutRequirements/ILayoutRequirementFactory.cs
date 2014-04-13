namespace Game.Logic.Requirements.LayoutRequirements
{
    public interface ILayoutRequirementFactory
    {
        AwayFromLayout CreateAwayFromLayout();

        SimpleLayout CreateSimpleLayout();
    }
}
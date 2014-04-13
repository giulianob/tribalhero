namespace Game.Data.Tribe
{
    public interface ITribeLogger
    {
        void Listen(ITribe tribe);
        void Unlisten(ITribe tribe);
    }
}
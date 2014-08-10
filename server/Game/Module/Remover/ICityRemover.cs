namespace Game.Module
{
    public interface ICityRemover
    {
        bool CanBeRemovedImmediately();
        bool Start(bool force = false);
    }
}
namespace Game.Module.Remover
{
    public interface IPlayersRemoverFactory
    {
        PlayersRemover CreatePlayersRemover(IPlayerSelector playerSelector);
    }
}
using Game.Data;

namespace Game.Module
{
    public interface ICityRemoverFactory {
        ICityRemover CreateCityRemover(ICity city);
    }
}
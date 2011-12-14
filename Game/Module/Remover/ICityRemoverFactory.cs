using Game.Data;

namespace Game.Module
{
    public interface ICityRemoverFactory {
        ICityRemover CreateCityRemover(City city);
    }
}
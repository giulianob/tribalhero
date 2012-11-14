using Game.Data;

namespace Game.Module
{
    class CityRemoverFactory : ICityRemoverFactory
    {
        public ICityRemover CreateCityRemover(ICity city)
        {
            return new CityRemover(city.Id);
        }
    }
}
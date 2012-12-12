namespace Game.Module
{
    public interface ICityRemoverFactory
    {
        CityRemover CreateCityRemover(uint cityId);
    }
}
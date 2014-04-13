namespace Game.Util
{
    public static class NumberExtensions
    {
        public static decimal FromPercentageDiscount(this int value)
        {
            return (100m - value) / 100m;
        }
    }
}

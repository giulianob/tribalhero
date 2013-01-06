namespace Game.Map
{
    public class MapMath
    {
        public delegate byte Distance(uint x, uint y, uint x1, uint y1);

        public uint AbsDiff(uint val1, uint val2)
        {
            return (val1 > val2 ? val1 - val2 : val2 - val1);
        }
    }
}
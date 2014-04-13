namespace Game.Map
{
    public class MapMath
    {
        /// <summary>
        ///     Returns whether two tiles are perpendicular. This means that they are on the same lines if you were to just draw
        ///     lines going up/down and left/right from a tile.
        /// </summary>
        public virtual bool IsPerpendicular(uint x, uint y, uint x1, uint y1)
        {
            return y == y1 || (x == x1 && y % 2 == y1 % 2);
        }
    }
}
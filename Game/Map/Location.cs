using System;

namespace Game.Map
{
    public class Location : IEquatable<Location>
    {
        public Location(uint x, uint y)
        {
            X = x;
            Y = y;
        }

        public uint X { get; set; }
        public uint Y { get; set; }

        #region IEquatable<Location> Members

        bool IEquatable<Location>.Equals(Location other)
        {
            return X == other.X && Y == other.Y;
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(Location))
                return false;
            return Equals((Location)obj);
        }

        public bool Equals(Location other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return other.X == X && other.Y == Y;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode()*397) ^ Y.GetHashCode();
            }
        }
    }
}
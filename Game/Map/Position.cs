using System;

namespace Game.Map
{
    public class Position : IEquatable<Position>
    {
        public Position(uint x, uint y)
        {
            X = x;
            Y = y;
        }

        public uint X { get; set; }

        public uint Y { get; set; }

        #region IEquatable<Location> Members

        bool IEquatable<Position>.Equals(Position other)
        {
            return X == other.X && Y == other.Y;
        }

        #endregion

        public bool Equals(Position other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return other.X == X && other.Y == Y;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != typeof(Position))
            {
                return false;
            }
            return Equals((Position)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode() * 397) ^ Y.GetHashCode();
            }
        }
    }
}
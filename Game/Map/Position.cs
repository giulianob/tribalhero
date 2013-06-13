using System;

namespace Game.Map
{
    public class Position : IEquatable<Position>, IComparable<Position>, IComparable
    {
        public Position(uint x, uint y)
        {
            X = x;
            Y = y;
        }

        public uint X { get; set; }

        public uint Y { get; set; }

        public virtual Position Top()
        {
            return new Position(X, Y - 2);
        }

        public virtual Position Bottom()
        {
            return new Position(X, Y + 2);
        }

        public virtual Position Left()
        {
            return new Position(X - 1, Y);
        }

        public virtual Position Right()
        {
            return new Position(X + 1, Y);
        }

        public virtual Position TopLeft()
        {
            return Y % 2 == 0 ? new Position(X - 1, Y - 1) : new Position(X, Y - 1);
        }

        public virtual Position TopRight()
        {
            return Y % 2 == 0 ? new Position(X, Y - 1) : new Position(X + 1, Y - 1);
        }

        public virtual Position BottomLeft()
        {
                return Y % 2 == 0 ? new Position(X - 1, Y + 1) : new Position(X, Y + 1);
        }

        public virtual Position BottomRight()
        {
            return Y % 2 == 0 ? new Position(X, Y + 1) : new Position(X + 1, Y + 1);
        }

        bool IEquatable<Position>.Equals(Position other)
        {
            return X == other.X && Y == other.Y;
        }

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

        public int CompareTo(Position other)
        {
            if (other == null)
            {
                return -1;
            }

            if (Equals(other))
            {
                return 0;
            }
            
            if (X < other.X)
            {
                return -1;
            }
            
            if (X > other.X)
            {
                return 1;
            }

            if (Y < other.Y)
            {
                return -1;
            }

            return 1;            
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

        public int CompareTo(object obj)
        {
            return CompareTo(obj as Position);
        }
    }
}
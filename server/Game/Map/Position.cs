using System;
using System.Diagnostics;

namespace Game.Map
{
    public struct Position : IEquatable<Position>, IComparable<Position>, IComparable
    {
        private uint x;

        private uint y;
        
        [DebuggerStepThrough]
        public Position(uint x, uint y)
        {
            this.x = x;
            this.y = y;
        }

        public uint X
        {
            get
            {
                return x;
            }
        }

        public uint Y
        {
            get
            {
                return y;
            }
        }

        public Position Top()
        {
            return new Position(X, Y - 2);
        }

        public Position Bottom()
        {
            return new Position(X, Y + 2);
        }

        public Position Left()
        {
            return new Position(X - 1, Y);
        }

        public Position Right()
        {
            return new Position(X + 1, Y);
        }

        public Position TopLeft()
        {
            return Y % 2 == 0 ? new Position(X - 1, Y - 1) : new Position(X, Y - 1);
        }

        public Position TopRight()
        {
            return Y % 2 == 0 ? new Position(X, Y - 1) : new Position(X + 1, Y - 1);
        }

        public Position BottomLeft()
        {
            return Y % 2 == 0 ? new Position(X - 1, Y + 1) : new Position(X, Y + 1);
        }

        public Position BottomRight()
        {
            return Y % 2 == 0 ? new Position(X, Y + 1) : new Position(X + 1, Y + 1);
        }

        
        bool IEquatable<Position>.Equals(Position other)
        {
            return X == other.X && Y == other.Y;
        }

        public bool Equals(Position other)
        {
            return other.X == X && other.Y == Y;
        }

        public int CompareTo(Position other)
        {
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
            if (obj.GetType() != typeof(Position))
            {
                return -1;
            }

            return CompareTo((Position) obj);
        }
    }
}
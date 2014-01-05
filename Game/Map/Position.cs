using System;
using System.ComponentModel;
using System.Diagnostics;
using Game.Util;

namespace Game.Map
{
    /// <summary>
    /// NOTE: Make all methods/properties virtual so that recursive mocks work by default without an interface
    /// </summary>
    public class Position : IEquatable<Position>, IComparable<Position>, IComparable
    {
        public virtual event EventHandler<PropertyChangedEventArgs> PropertyChanged;

        private uint x;

        private uint y;
        
        [DebuggerStepThrough]
        public Position()
        {            
        }

        [DebuggerStepThrough]
        public Position(uint x, uint y)
        {
            this.x = x;
            this.y = y;
        }

        public virtual uint X
        {
            get
            {
                return x;
            }

            set
            {
                x = value;
                PropertyChanged.Raise(this, new PropertyChangedEventArgs("X"));
            }
        }
        
        public virtual uint Y
        {
            get
            {
                return y;
            }

            set
            {
                y = value;
                PropertyChanged.Raise(this, new PropertyChangedEventArgs("Y"));
            }
        }

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

        public virtual bool Equals(Position other)
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

        public virtual int CompareTo(Position other)
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

        public virtual int CompareTo(object obj)
        {
            return CompareTo(obj as Position);
        }

        public virtual Position Clone()
        {
            return new Position(x, y);
        }
    }
}
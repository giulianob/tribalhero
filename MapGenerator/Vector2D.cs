using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapGenerator
{
    public class Vector2D
    {
        public Vector2D(uint x, uint y)
        {
            X = x;
            Y = y;
        }

        public uint X { get; private set; }

        public uint Y { get; private set; }
    }
}

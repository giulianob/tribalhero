#region



#endregion

using System.Collections.Generic;
using Game.Data;

namespace Game.Map
{
    public class MapMath
    {
        public static uint AbsDiff(uint val1, uint val2)
        {
            return (val1 > val2 ? val1 - val2 : val2 - val1);
        }

        public static bool IsOverlapping(Location location1, byte r1, Location location2, byte r2)
        {
            HashSet<Location> points = new HashSet<Location>();

            RadiusLocator.ForeachObject(location1.X,
                                        location1.Y,
                                        r1,
                                        true,
                                        (x, y, u, u1, custom) =>
                                            {
                                                points.Add(new Location(u, u1));
                                                return true;
                                            },
                                        null);

            bool overlapping = false;

            RadiusLocator.ForeachObject(location2.X,
                                        location2.Y,
                                        r2,
                                        true,
                                        (x, y, u, u1, custom) =>
                                        {
                                            if (points.Contains(new Location(u, u1)))
                                            {
                                                overlapping = true;
                                                return false;
                                            }

                                            return true;
                                        },
                                        null);

            return overlapping;
        }
    }
}
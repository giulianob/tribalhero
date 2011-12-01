using System;
using System.IO;
using FluentAssertions;
using Game.Data;
using Game.Map;
using Xunit;
using Xunit.Extensions;

namespace Testing.MapMath
{
    public class MapMathTest
    {
        [Fact]
        public void Generate()
        {
            using (var file = new StreamWriter(File.Create(@"C:\Users\OscarMike\out.txt")))
            {
                file.WriteLine("X1,Y1,R1,X2,Y2,R2,OVERLAPPING,DISTANCE,EQUALSUMTODISTANCE");
                Location even = new Location(1671, 1721);
          /*      RadiusLocator.ForeachObject(even.X,even.Y,3, true, (x,y,u,u1,custom)=>
                    {
                        file.WriteLine("{0},{1}", u, u1);
                        return true;
                    },null);*/
                RadiusLocator.ForeachObject(even.X,
                                            even.Y,
                                            8,
                                            false,
                                            (x, y, u, u1, custom) =>
                                                {
                                                    var location = new Location(u, u1);
                                                    for (byte i = 0; i <= 8; i++)
                                                    {
                                                        for (byte j = 0; j <= 8; j++)
                                                        {
                                                            var overlapping = Game.Map.MapMath.IsOverlapping(even, i, location, j);
                                                            var distance = SimpleGameObject.RadiusDistance(even.X, even.Y, location.X, location.Y);
                                                            var sumEqualToDistance = distance - 1 == i + j;
                                                            var lessThan = distance - 1 < i + j;

                                                            if (!sumEqualToDistance)
                                                            {
                                                                continue;
                                                            }

                                                            file.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                                                                           even.X,
                                                                           even.Y,
                                                                           i,
                                                                           location.X,
                                                                           location.Y,
                                                                           j,
                                                                           overlapping ? "Y" : "N",
                                                                           distance,
                                                                           sumEqualToDistance ? "Y" : "N",
                                                                           lessThan ? "Y" : "N");
                                                        }
                                                    }

                                                    return true;
                                                },
                                            null);
            }
        }

/*        [Theory]
        public void TestIsOverlapping(uint x1, uint y1, byte r1, uint x2, uint y2, byte r2, bool expected)
        {
            Game.Map.MapMath.IsOverlapping(distance, radius1, radius2).Should().Be(expected);
        }*/
    }
}

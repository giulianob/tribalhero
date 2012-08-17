#region

using System.Collections.Generic;
using System.Linq;
using Game.Data;

#endregion

namespace Game.Map
{
    public class RoadPathFinder
    {
        public static bool HasPath(Location start, Location end, ICity city, Location excludedPoint)
        {
            bool fromStructure = World.Current[start.X, start.Y].Exists(s => s is IStructure);

            return BreadthFirst(new Location(end.X, end.Y),
                                new List<Location> {new Location(start.X, start.Y)},
                                excludedPoint,
                                delegate(Location node)
                                    {
                                        var neighbors = new List<Location>(4);
                                        Location[] possibleNeighbors;
                                        if (node.Y%2 == 0)
                                        {
                                            possibleNeighbors = new[]
                                                                {
                                                                        new Location(node.X, node.Y - 1), new Location(node.X, node.Y + 1),
                                                                        new Location(node.X - 1, node.Y - 1), new Location(node.X - 1, node.Y + 1)
                                                                };
                                        }
                                        else
                                        {
                                            possibleNeighbors = new[]
                                                                {
                                                                        new Location(node.X + 1, node.Y - 1), new Location(node.X + 1, node.Y + 1),
                                                                        new Location(node.X, node.Y - 1), new Location(node.X, node.Y + 1)
                                                                };
                                        }

                                        neighbors.AddRange(possibleNeighbors.Where(delegate(Location location)
                                            {
                                                if (!location.Equals(end))
                                                {
                                                    if (World.Current[location.X, location.Y].Exists(s => s is IStructure))
                                                        return false;
                                                    if (!World.Current.Roads.IsRoad(location.X, location.Y))
                                                        return false;
                                                    if (SimpleGameObject.TileDistance(location.X, location.Y, city.X, city.Y) >
                                                        city.Radius)
                                                        return false;
                                                }
                                                else if (fromStructure && node.Equals(start))
                                                    return false;

                                                return true;
                                            }));

                                        return neighbors;
                                    });
        }

        private static bool BreadthFirst(Location end, List<Location> visited, Location excludedPoint, GetNeighbors getNeighbors)
        {
            List<Location> nodes = getNeighbors(visited.Last());

            // Examine adjacent nodes for end goal
            if (nodes.Contains(end))
                return true;

            // in breadth-first, recursion needs to come after visiting adjacent nodes
            foreach (var node in
                    nodes.Where(node => !node.Equals(end) && !node.Equals(excludedPoint) && !visited.Contains(node)))
            {
                visited.Add(node);
                if (BreadthFirst(end, visited, excludedPoint, getNeighbors))
                    return true;
            }

            return false;
        }

        #region Nested type: GetNeighbors

        private delegate List<Location> GetNeighbors(Location node);

        #endregion
    }
}
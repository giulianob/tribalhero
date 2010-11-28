using System.Collections.Generic;
using System.Linq;
using Game.Data;

namespace Game.Map
{
    public class RoadPathFinder
    {
        delegate List<Location> GetNeighbors(Location node);

        public static bool HasPath(Location start, Location end, City city, Location excludedPoint)
        {
            bool fromStructure = Global.World[start.x, start.y].Exists(s => s is Structure);

            return BreadthFirst(new Location(end.x, end.y), new List<Location> { new Location(start.x, start.y) }, excludedPoint,
                delegate(Location node)
                {
                    List<Location> neighbors = new List<Location>(4);
                    Location[] possibleNeighbors;
                    if (node.y % 2 == 0)
                    {
                        possibleNeighbors = new[] { new Location(node.x, node.y - 1), new Location(node.x, node.y + 1), new Location(node.x - 1, node.y - 1), new Location(node.x - 1, node.y + 1) };
                    }
                    else
                    {
                        possibleNeighbors = new[] { new Location(node.x + 1, node.y - 1), new Location(node.x + 1, node.y + 1), new Location(node.x, node.y - 1), new Location(node.x, node.y + 1) };
                    }

                    neighbors.AddRange(possibleNeighbors.Where(delegate(Location location)
                    {
                        if (!location.Equals(end))
                        {
                            if (Global.World[location.x, location.y].Exists(s => s is Structure))
                                return false;
                            if (!RoadManager.IsRoad(location.x, location.y))
                                return false;
                            if (SimpleGameObject.TileDistance(location.x, location.y, city.MainBuilding.X, city.MainBuilding.Y) > city.Radius)
                                return false;
                        }
                        else if (fromStructure && node.Equals(start))
                        {
                            return false;
                        }

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
            foreach (Location node in nodes.Where(node => !node.Equals(end) && !node.Equals(excludedPoint) && !visited.Contains(node)))
            {
                visited.Add(node);
                if (BreadthFirst(end, visited, excludedPoint, getNeighbors))
                    return true;
            }

            return false;
        }
    }
}
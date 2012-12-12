#region

using System.Collections.Generic;
using System.Linq;
using Game.Data;

#endregion

namespace Game.Map
{
    public class RoadPathFinder
    {
        public static bool HasPath(Position start, Position end, ICity city, Position excludedPoint)
        {
            bool fromStructure = World.Current[start.X, start.Y].Exists(s => s is IStructure);

            return BreadthFirst(new Position(end.X, end.Y),
                                new List<Position> {new Position(start.X, start.Y)},
                                excludedPoint,
                                delegate(Position node)
                                    {
                                        var neighbors = new List<Position>(4);
                                        Position[] possibleNeighbors;
                                        if (node.Y % 2 == 0)
                                        {
                                            possibleNeighbors = new[]
                                            {
                                                    new Position(node.X, node.Y - 1), new Position(node.X, node.Y + 1),
                                                    new Position(node.X - 1, node.Y - 1),
                                                    new Position(node.X - 1, node.Y + 1)
                                            };
                                        }
                                        else
                                        {
                                            possibleNeighbors = new[]
                                            {
                                                    new Position(node.X + 1, node.Y - 1), new Position(node.X + 1, node.Y + 1),
                                                    new Position(node.X, node.Y - 1), new Position(node.X, node.Y + 1)
                                            };
                                        }

                                        neighbors.AddRange(possibleNeighbors.Where(delegate(Position location)
                                            {
                                                if (!location.Equals(end))
                                                {
                                                    if (
                                                            World.Current[location.X, location.Y].Exists(
                                                                                                         s =>
                                                                                                         s is IStructure))
                                                    {
                                                        return false;
                                                    }
                                                    if (!World.Current.Roads.IsRoad(location.X, location.Y))
                                                    {
                                                        return false;
                                                    }
                                                    if (
                                                            SimpleGameObject.TileDistance(location.X,
                                                                                          location.Y,
                                                                                          city.X,
                                                                                          city.Y) > city.Radius)
                                                    {
                                                        return false;
                                                    }
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

        private static bool BreadthFirst(Position end,
                                         List<Position> visited,
                                         Position excludedPoint,
                                         GetNeighbors getNeighbors)
        {
            List<Position> nodes = getNeighbors(visited.Last());

            // Examine adjacent nodes for end goal
            if (nodes.Contains(end))
            {
                return true;
            }

            // in breadth-first, recursion needs to come after visiting adjacent nodes
            foreach (var node in
                    nodes.Where(node => !node.Equals(end) && !node.Equals(excludedPoint) && !visited.Contains(node)))
            {
                visited.Add(node);
                if (BreadthFirst(end, visited, excludedPoint, getNeighbors))
                {
                    return true;
                }
            }

            return false;
        }

        #region Nested type: GetNeighbors

        private delegate List<Position> GetNeighbors(Position node);

        #endregion
    }
}
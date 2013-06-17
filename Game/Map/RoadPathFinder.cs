#region

using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Setup;

#endregion

namespace Game.Map
{
    public class RoadPathFinder : IRoadPathFinder
    {
        private readonly IWorld world;

        private readonly TileLocator tileLocator;

        private readonly ObjectTypeFactory objectTypeFactory;

        private readonly RadiusLocator radiusLocator;

        public RoadPathFinder(IWorld world, TileLocator tileLocator, ObjectTypeFactory objectTypeFactory, RadiusLocator radiusLocator)
        {
            this.world = world;
            this.tileLocator = tileLocator;
            this.objectTypeFactory = objectTypeFactory;
            this.radiusLocator = radiusLocator;
        }

        public Error CanBuild(uint x, uint y, ICity city, bool requiresRoad)
        {
            return Error.Ok;

            bool buildingOnRoad = world.Roads.IsRoad(x, y);

            if (requiresRoad)
            {
                if (buildingOnRoad)
                {
                    bool breaksRoad = false;

                    foreach (var str in city)
                    {
                        if (str.IsMainBuilding)
                        {
                            continue;
                        }

                        if (objectTypeFactory.IsObjectType("NoRoadRequired", str.Type))
                        {
                            continue;
                        }

                        if (!HasPath(new Position(str.X, str.Y),
                                                    new Position(city.X, city.Y),
                                                    city,
                                                    new Position(x, y)))
                        {
                            breaksRoad = true;
                            break;
                        }
                    }

                    if (breaksRoad)
                    {
                        world.Regions.UnlockRegion(x, y);
                        return Error.RoadDestroyUniquePath;
                    }

                    // Make sure all neighboring roads have a diff path
                    bool allNeighborsHaveOtherPaths = true;
                    radiusLocator.ForeachObject(x,
                                                y,
                                                1,
                                                false,
                                                (origX, origY, x1, y1, custom) =>
                                                    {
                                                        if (radiusLocator.RadiusDistance(origX, origY, x1, y1) != 1)
                                                        {
                                                            return true;
                                                        }

                                                        if (city.X == x1 && city.Y == y1)
                                                        {
                                                            return true;
                                                        }

                                                        if (world.Roads.IsRoad(x1, y1))
                                                        {
                                                            if (!HasPath(new Position(x1, y1),
                                                                                        new Position(city.X, city.Y),
                                                                                        city,
                                                                                        new Position(origX, origY)))
                                                            {
                                                                allNeighborsHaveOtherPaths = false;
                                                                return false;
                                                            }
                                                        }

                                                        return true;
                                                    },
                                                null);

                    if (!allNeighborsHaveOtherPaths)
                    {
                        world.Regions.UnlockRegion(x, y);
                        return Error.RoadDestroyUniquePath;
                    }
                }

                bool hasRoad = false;

                radiusLocator.ForeachObject(x,
                                            y,
                                            1,
                                            false,
                                            delegate(uint origX, uint origY, uint x1, uint y1, object custom)
                                                {
                                                    if (radiusLocator.RadiusDistance(origX, origY, x1, y1) != 1)
                                                    {
                                                        return true;
                                                    }

                                                    var curStruct =
                                                            (IStructure)
                                                            world[x1, y1].FirstOrDefault(obj => obj is IStructure);

                                                    bool hasStructure = curStruct != null;

                                                    // Make sure we have a road around this building
                                                    if (!hasRoad && !hasStructure && world.Roads.IsRoad(x1, y1))
                                                    {
                                                        if (!buildingOnRoad || HasPath(new Position(x1, y1),
                                                                                                      new Position(city.X, city.Y),
                                                                                                      city,
                                                                                                      new Position(origX, origY)))
                                                        {
                                                            hasRoad = true;
                                                        }
                                                    }

                                                    return true;
                                                },
                                            null);

                if (!hasRoad)
                {
                    world.Regions.UnlockRegion(x, y);
                    return Error.RoadNotAround;
                }
            }
            else
            {
                // Cant build on road if this building doesnt require roads
                if (buildingOnRoad)
                {
                    world.Regions.UnlockRegion(x, y);
                    return Error.RoadDestroyUniquePath;
                }
            }

            return Error.Ok;
        }

        public bool HasPath(Position start, Position end, ICity city, Position excludedPoint)
        {
            bool fromStructure = world[start.X, start.Y].Exists(s => s is IStructure);

            return BreadthFirst(new Position(end.X, end.Y),
                                new List<Position> {new Position(start.X, start.Y)},
                                excludedPoint,
                                node =>
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

                                        neighbors.AddRange(possibleNeighbors.Where(location =>
                                            {
                                                if (!location.Equals(end))
                                                {
                                                    if (world[location.X, location.Y].Exists(s => s is IStructure))
                                                    {
                                                        return false;
                                                    }
                                                    if (!world.Roads.IsRoad(location.X, location.Y))
                                                    {
                                                        return false;
                                                    }
                                                    if (tileLocator.TileDistance(location.X,
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

        private bool BreadthFirst(Position end,
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
            foreach (var node in nodes.Where(node => !node.Equals(end) && !node.Equals(excludedPoint) && !visited.Contains(node)))
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
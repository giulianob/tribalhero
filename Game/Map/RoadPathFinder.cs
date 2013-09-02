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

        private readonly ITileLocator tileLocator;

        private readonly IObjectTypeFactory objectTypeFactory;

        public RoadPathFinder(IWorld world, ITileLocator tileLocator, IObjectTypeFactory objectTypeFactory)
        {
            this.world = world;
            this.tileLocator = tileLocator;
            this.objectTypeFactory = objectTypeFactory;
        }

        public Error CanBuild(SimpleGameObject gameObject, ICity city, bool requiresRoad)
        {
            var buildingPositions = tileLocator.ForeachMultitile(gameObject).ToArray();
            var buildingNeighbors = tileLocator.ForeachRadius(gameObject, 1).ToArray();

            var mainBuildingPositions = tileLocator.ForeachMultitile(city.MainBuilding).ToArray();

            var roadsBeingBuiltOn = buildingPositions.Where(buildingPosition => world.Roads.IsRoad(buildingPosition.X, buildingPosition.Y)).ToArray();

            if (!requiresRoad)
            {
                // Cant build on road if this building doesnt require roads
                if (roadsBeingBuiltOn.Any())
                {
                    return Error.RoadDestroyUniquePath;
                }

                return Error.Ok;
            }

            if (roadsBeingBuiltOn.Any())
            {
                // Make sure all structures still have a valid path if we are building on top of a road
                foreach (var str in city)
                {
                    if (str.IsMainBuilding || objectTypeFactory.IsObjectType("NoRoadRequired", str.Type))
                    {
                        continue;
                    }

                    if (!HasPath(start: new Position(str.PrimaryPosition.X, str.PrimaryPosition.Y),
                                 end: new Position(city.PrimaryPosition.X, city.PrimaryPosition.Y),
                                 excludedPoints: buildingPositions, city: city))
                    {
                        return Error.RoadDestroyUniquePath;
                    }
                }

                // Make sure all neighboring roads have a path
                foreach (var neighborPosition in buildingNeighbors)
                {
                    if (mainBuildingPositions.Contains(neighborPosition) || !world.Roads.IsRoad(neighborPosition.X, neighborPosition.Y))
                    {
                        continue;
                    }

                    if (!HasPath(start: new Position(neighborPosition.X, neighborPosition.Y),
                                 end: new Position(city.PrimaryPosition.X, city.PrimaryPosition.Y),
                                 excludedPoints: buildingPositions, 
                                 city: city))
                    {
                        return Error.RoadDestroyUniquePath;
                    }
                }
            }

            // Make sure we have a road around this building
            bool hasRoad = false;
            foreach (var neighborPosition in buildingNeighbors)
            {
                bool hasStructure = world.Regions.GetObjectsInTile(neighborPosition.X, neighborPosition.Y)
                                         .Any(obj => obj is IStructure);
                
                if (hasStructure || !world.Roads.IsRoad(neighborPosition.X, neighborPosition.Y))
                {
                    continue;
                }

                if (!roadsBeingBuiltOn.Any() || HasPath(start: new Position(neighborPosition.X, neighborPosition.Y),
                                               end: new Position(city.PrimaryPosition.X, city.PrimaryPosition.Y),
                                               excludedPoints: buildingPositions, 
                                               city: city))
                {
                    hasRoad = true;
                    break;
                }
            }

            return !hasRoad ? Error.RoadNotAround : Error.Ok;
        }

        public bool HasPath(Position start, Position end, IEnumerable<Position> excludedPoints, ICity city)
        {
            bool fromStructure = world.Regions.GetObjectsInTile(start.X, start.Y).Any(s => s is IStructure);

            return BreadthFirst(new Position(end.X, end.Y),
                                new List<Position> {new Position(start.X, start.Y)},
                                excludedPoints,
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
                                                if (world.Regions.GetObjectsInTile(location.X, location.Y).Any(s => s is IStructure))
                                                {
                                                    return false;
                                                }

                                                if (!world.Roads.IsRoad(location.X, location.Y))
                                                {
                                                    return false;
                                                }

                                                if (tileLocator.TileDistance(location, 1, city.PrimaryPosition, 1) > city.Radius)
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
                                  IEnumerable<Position> excludedPoints,
                                  GetNeighbors getNeighbors)
        {
            List<Position> nodes = getNeighbors(visited.Last());

            // Examine adjacent nodes for end goal
            if (nodes.Contains(end))
            {
                return true;
            }

            // in breadth-first, recursion needs to come after visiting adjacent nodes
            foreach (var node in nodes.Where(node => !node.Equals(end) && !excludedPoints.Contains(node) && !visited.Contains(node)))
            {
                visited.Add(node);

                if (BreadthFirst(end, visited, excludedPoints, getNeighbors))
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
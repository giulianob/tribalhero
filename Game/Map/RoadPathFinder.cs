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

        public Error CanBuild(Position position, byte size, ICity city, bool requiresRoad)
        {
            var buildingPositions = tileLocator.ForeachMultitile(position.X, position.Y, size).ToArray();
            var buildingNeighbors = tileLocator.ForeachRadius(position.X, position.Y, size, 1).ToArray();

            var mainBuildingPositions = tileLocator.ForeachMultitile(city.MainBuilding).ToArray();

            var roadsBeingBuiltOn = buildingPositions.Where(buildingPosition => world.Roads.IsRoad(buildingPosition.X, buildingPosition.Y)).ToArray();

            if (!requiresRoad)
            {
                // Cant build on road if this building doesnt require roads
                return roadsBeingBuiltOn.Any() ? Error.RoadDestroyUniquePath : Error.Ok;
            }

            if (roadsBeingBuiltOn.Any())
            {
                // All structures should still have a valid path if we are building on top of a road
                foreach (var str in city)
                {
                    if (str.IsMainBuilding || objectTypeFactory.IsObjectType("NoRoadRequired", str.Type))
                    {
                        continue;
                    }

                    if (!HasPath(start: str.PrimaryPosition,
                                 startSize: str.Size,
                                 excludedPoints: buildingPositions, city: city))
                    {
                        return Error.RoadDestroyUniquePath;
                    }
                }

                // All neighboring roads should have a different path
                foreach (var neighborPosition in buildingNeighbors)
                {
                    if (mainBuildingPositions.Contains(neighborPosition) || !world.Roads.IsRoad(neighborPosition.X, neighborPosition.Y))
                    {
                        continue;
                    }

                    if (!HasPath(start: new Position(neighborPosition.X, neighborPosition.Y),
                                 startSize: 1,
                                 excludedPoints: buildingPositions,
                                 city: city))
                    {
                        return Error.RoadDestroyUniquePath;
                    }
                }
            }

            // There should be a road around this building
            bool hasRoad = false;
            foreach (var neighborPosition in buildingNeighbors)
            {
                bool hasStructure = world.Regions.GetObjectsInTile(neighborPosition.X, neighborPosition.Y)
                                         .Any(obj => obj is IStructure);
                
                if (hasStructure || !world.Roads.IsRoad(neighborPosition.X, neighborPosition.Y))
                {
                    continue;
                }

                if (roadsBeingBuiltOn.Any() && !HasPath(start: new Position(neighborPosition.X, neighborPosition.Y),
                                                        startSize: 1,
                                                        excludedPoints: buildingPositions,
                                                        city: city))
                {
                    continue;
                }

                hasRoad = true;
                break;
            }

            return !hasRoad ? Error.RoadNotAround : Error.Ok;
        }

        public bool HasPath(Position start, byte startSize, ICity city, IEnumerable<Position> excludedPoints)
        {
            var fromStructure = world.Regions.GetObjectsInTile(start.X, start.Y).OfType<IStructure>().Any();
            var startPositions = tileLocator.ForeachMultitile(start.X, start.Y, startSize).ToList();
            var mainBuilding = city.MainBuilding;
            var mainBuildingPositions = tileLocator.ForeachMultitile(mainBuilding).ToList();

            if (startPositions.Intersect(mainBuildingPositions).Any())
            {
                return true;
            }

            return BreadthFirst(end: tileLocator.ForeachMultitile(mainBuilding.PrimaryPosition.X, mainBuilding.PrimaryPosition.Y, mainBuilding.Size).ToList(),
                                visited: new List<Position> {new Position(start.X, start.Y)},
                                excludedPoints: excludedPoints,
                                getNeighbors: node =>
                                {
                                    var possibleNeighbors = new[]
                                    {
                                        node.TopLeft(),
                                        node.TopRight(),
                                        node.BottomLeft(),
                                        node.BottomRight()
                                    };

                                    return possibleNeighbors.Where(location =>
                                    {
                                        // If the neighbor we are checking is where the mainbuilding is then we've found the goal
                                        if (mainBuildingPositions.Contains(location))
                                        {
                                            // We have only found the target if the structure we're checking is not directly next to the main building
                                            return !fromStructure || !startPositions.Contains(node);
                                        }

                                        if (startPositions.Contains(location))
                                        {
                                            return true;
                                        }

                                        if (tileLocator.TileDistance(location, 1, city.PrimaryPosition, 1) > city.Radius)
                                        {
                                            return false;
                                        }

                                        var structureOnTile = world.Regions.GetObjectsInTile(location.X, location.Y).OfType<IStructure>().FirstOrDefault();
                                        if (structureOnTile != null)
                                        {
                                            return false;
                                        }

                                        return world.Roads.IsRoad(location.X, location.Y);
                                    }).ToList();
                                });
        }

        private bool BreadthFirst(List<Position> end,
                                  List<Position> visited,
                                  IEnumerable<Position> excludedPoints,
                                  GetNeighbors getNeighbors)
        {
            List<Position> nodes = getNeighbors(visited.Last());
            // Examine adjacent nodes for end goal
            if (nodes.Intersect(end).Any())
            {
                return true;
            }

            // in breadth-first, recursion needs to come after visiting adjacent nodes
            foreach (var node in nodes.Where(node => !excludedPoints.Contains(node) && !visited.Contains(node)))
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
package src.Map
{
    import System.Linq.Enumerable;

    import src.Constants;
    import src.Global;
    import src.Objects.Factories.ObjectFactory;

    public class RoadPathFinder
	{
        private static var positionComparer: PositionComparer = new PositionComparer();

        public static function CanBuild(position: Position, size: int, city: City, requiresRoad: Boolean): Boolean {
            var buildingPositions: Array = TileLocator.foreachMultitile(position.x, position.y, size);
            var buildingNeighbors: Array = TileLocator.foreachRadiusWithSize(position.x, position.y, size, 1);

            var mainBuildingPositions: Array = TileLocator.foreachMultitile(city.MainBuilding.primaryPosition.x, city.MainBuilding.primaryPosition.y, city.MainBuilding.size);

            var roadsBeingBuiltOn: Array = Enumerable.from(buildingPositions).where(function (buildingPosition: Position): Boolean {
               return isRoadByMapPosition(buildingPosition);
            }).toArray();

            if (!requiresRoad) {
                return roadsBeingBuiltOn.length == 0;
            }

            if (roadsBeingBuiltOn.length > 0) {
                for each (var str: CityObject in city.structures()) {
                    if (str.isMainBuilding || ObjectFactory.isType("NoRoadsRequired", str.type)) {
                        continue;
                    }

                    if (!hasPath(str.primaryPosition, str.size, city, buildingPositions)) {
                        return false;
                    }
                }

                for each (var neighborPosition: Position in buildingNeighbors) {
                    if (Enumerable.from(mainBuildingPositions).contains(neighborPosition, positionComparer) || !isRoadByMapPosition(neighborPosition)) {
                        continue;
                    }

                    if (!hasPath(neighborPosition, 1, city, buildingPositions)) {
                        return false;
                    }
                }
            }

            for each (neighborPosition in buildingNeighbors) {
                var hasStructure: Boolean = city.getStructureAt(neighborPosition) != null;

                if (hasStructure || !isRoadByMapPosition(neighborPosition)) {
                    continue;
                }

                if (roadsBeingBuiltOn.length > 0 && !hasPath(neighborPosition, 1, city, buildingPositions)) {
                    continue;
                }

                return true;
            }

            return false;
        }

		public static function isRoadByMapPosition(position: Position) : Boolean {
			return isRoad(Global.map.regions.getTileAt(position));
		}

		public static function isRoad(tileId: int) : Boolean {
			return tileId >= Constants.road_start_tile_id && tileId <= Constants.road_end_tile_id;
		}

        public static function hasPath(start: Position, size: int, city: City, excludedPoints: Array) : Boolean {
            var fromStructure: Boolean = city.hasStructureAt(start);
            var startPositions: Array = TileLocator.foreachMultitile(start.x, start.y, size);
            var mainBuilding: CityObject = city.MainBuilding;
            var mainBuildingPositions: Array = TileLocator.foreachMultitile(mainBuilding.primaryPosition.x, mainBuilding.primaryPosition.y, mainBuilding.size);

            if (TileLocator.containsIntersectingPoints(startPositions, mainBuildingPositions)) {
                return true;
            }

            return breadthFirst(
                    mainBuildingPositions,
                    [ start ],
                    excludedPoints,
                    function(node : Position) : Array
                    {
                        //new code
                        var possibleNeighbors: Array = [
                            node.topLeft(),
                            node.topRight(),
                            node.bottomLeft(),
                            node.bottomRight()
                        ];

                        return Enumerable.from(possibleNeighbors).where(function (location: Position): Boolean {
                            var inStartPosition: Boolean = Enumerable.from(startPositions).contains(location, positionComparer);

                            if (Enumerable.from(mainBuildingPositions).contains(location, positionComparer)) {
                                return !fromStructure || !inStartPosition;
                            }

                            if (inStartPosition) {
                                return true;
                            }

                            if (city.hasStructureAt(location)) {
                                return false;
                            }

                            return isRoadByMapPosition(location);
                        }).toArray();
                    });
        }

		private static function breadthFirst(end: Array, visited: Array, excludedPoints: Array, getNeighbors: Function) : Boolean
		{
			var nodes: Array = getNeighbors(visited[visited.length - 1]);

            if (TileLocator.containsIntersectingPoints(nodes, end)) {
                return true;
            }

			// Sort neighbors by distance. Helps out performance a bit.
			nodes.sort(function (a:Position, b:Position):Number {
				var aDist: Number = TileLocator.distance(a.x, a.y, 1, end.x, end.y, 1);
				var bDist: Number = TileLocator.distance(b.x, b.y, 1, end.x, end.y, 1);

				if(aDist > bDist) return 1;
				if (aDist < bDist) return -1;

				return 0;
			});

			// Search nodes for goal
			for each (var node: Position in nodes) {
                if (Enumerable.from(excludedPoints).contains(node, positionComparer) || Enumerable.from(visited).contains(node, positionComparer))
                {
                    continue;
                }

                visited.push(node);
                if (breadthFirst(end, visited, excludedPoints, getNeighbors)) {
                    return true;
                }
			}

			return false;
		}
	}

}


package src.Map
{
	import flash.geom.Point;
	import src.Constants;
	import src.Global;
    import src.Objects.Factories.ObjectFactory;

    public class RoadPathFinder
	{
		private static function hasPoint(arr: Array, position: Position) : Boolean {

			var cnt: int = arr.length;

			for (var i: int = 0; i < cnt; i++) {
				var item: Position = arr[i];
				if (item.x == position.x && item.y == position.y) return true;
			}

			return false;
		}

        public static function CanBuild(mapPos: Position, city: City, requiredRoad: Boolean): Boolean {
            return true;

            var buildingOnRoad: Boolean = RoadPathFinder.isRoad(Global.map.regions.getTileAt(mapPos.x, mapPos.y));

            if (!requiredRoad) {
                // Don't allow structures that don't need roads to be built on top of roads
                return !buildingOnRoad;
            }

            // Keep non road related checks above this
            // Check for road requirement
            if (buildingOnRoad) {
                var breaksPath: Boolean = false;
                for each(var cityObject: CityObject in city.objects) {
                    if (cityObject.x == city.MainBuilding.x && cityObject.y == city.MainBuilding.y) continue;
                    if (ObjectFactory.isType("NoRoadRequired", cityObject.type)) continue;

                    if (!RoadPathFinder.hasPath(new Position(cityObject.x, cityObject.y), new Position(city.MainBuilding.x, city.MainBuilding.y), city, mapPos)) {
                        breaksPath = true;
                        break;
                    }
                }

                if (breaksPath) return false;

                // Make sure all neighbors have a different path
                for each (var position: Position in TileLocator.foreachTile(mapPos.x, mapPos.y, 1, false))
                {
                    if (TileLocator.radiusDistance(mapPos.x, mapPos.y, position.x, position.y) != 1) {
                        continue;
                    }

                    if (city.MainBuilding.x == position.x && city.MainBuilding.y == position.y) {
                        continue;
                    }

                    if (RoadPathFinder.isRoadByMapPosition(position.x, position.y)) {
                        if (!RoadPathFinder.hasPath(new Position(position.x, position.y), new Position(city.MainBuilding.x, city.MainBuilding.y), city, mapPos)) {
                            return false;
                        }
                    }
                }
            }

            var hasRoad: Boolean = false;

            for each (var position: Position in TileLocator.foreachTile(mapPos.x, mapPos.y, 1, false)) {
                if (TileLocator.radiusDistance(mapPos.x, mapPos.y, position.x, position.y) != 1) {
                    continue;
                }

                var structure: CityObject = city.getStructureAt(new Position(position.x, position.y));

                var hasStructure: Boolean = structure != null;

                // Make sure we have a road around this building
                if (!hasRoad && !hasStructure && RoadPathFinder.isRoadByMapPosition(position.x, position.y)) {
                    // If we are building on road, we need to check that all neighbor tiles have another connection to the main building
                    if (!buildingOnRoad || RoadPathFinder.hasPath(new Position(position.x, position.y), new Position(city.MainBuilding.x, city.MainBuilding.y), city, mapPos)) {
                        hasRoad = true;
                    }
                }
            }

            return hasRoad;
        }

		public static function isRoadByMapPosition(x: int, y: int) : Boolean {
			return isRoad(Global.map.regions.getTileAt(x, y));
		}

		public static function isRoad(tileId: int) : Boolean {
			return tileId >= Constants.road_start_tile_id && tileId <= Constants.road_end_tile_id;
		}

		public static function hasPath(start: Position, end: Position, city: City, excludedPoint: Position) : Boolean {

			if (start.x == end.x && start.y == end.y) return true;

			var visited: Array = [];

			visited.push(start);

			var fromStructure: Boolean = city.hasStructureAt(start);

			var ret: * = breadthFirst(new Point(end.x, end.y), visited, function(node : Point) : Array
			{
				var neighbors: Array = [];
				var possibleNeighbors: Array;
				if (node.y % 2 == 0)
				{
					possibleNeighbors = new Array(
					new Position(node.x, node.y - 1),
					new Position(node.x, node.y + 1),
					new Position(node.x - 1, node.y - 1),
					new Position(node.x - 1, node.y + 1)
					);
				}
				else
				{
					possibleNeighbors = new Array(
					new Position(node.x + 1, node.y - 1),
					new Position(node.x + 1, node.y + 1),
					new Position(node.x, node.y - 1),
					new Position(node.x, node.y + 1)
					);
				}

				for each (var location: Position in possibleNeighbors) {
					if ((location.x != end.x || location.y != end.y)) {
						if (hasPoint(visited, location)) continue;
						if (!isRoadByMapPosition(location.x, location.y)) continue;
						if (city.hasStructureAt(location)) continue;
						if (TileLocator.distance(location.x, location.y, city.MainBuilding.x, city.MainBuilding.y) > city.radius) continue;
					} else if (fromStructure && node.x == start.x && node.y == start.y) {
						continue;
					}

					neighbors.push(location);
				}

				return neighbors;
			}, excludedPoint);
			
			return ret;
		}

		private static function breadthFirst(end: Point, visited: Array, getNeighbors: Function, excludedPoint: Position, i: int = 0) : Boolean
		{
			//Util.log("Checking " + visited[visited.length - 1].toString());

			var nodes: Array = getNeighbors(visited[visited.length - 1]);

			// Examine adjacent nodes for end goal
			for each (var node: Point in nodes) {
				if ((node.x == end.x && node.y == end.y)) {
					return true;
				}
			}

			// Sort neighbors by distance. Helps out a bit.
			nodes.sort(function (a:Point, b:Point):Number {
				var aDist: Number = TileLocator.distance(a.x, a.y, end.x, end.y);
				var bDist: Number = TileLocator.distance(b.x, b.y, end.x, end.y);

				if(aDist > bDist) return 1;
				if (aDist < bDist) return -1;

				return 0;
			});

			// Search nodes for goal
			for each (node in nodes) {
				if (!(node.x == excludedPoint.x && node.y == excludedPoint.y)) {
					visited.push(node);
					if (breadthFirst(end, visited, getNeighbors, excludedPoint, i)) return true;
				}
			}

			return false;
		}
	}

}


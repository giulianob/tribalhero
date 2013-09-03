package src.Map
{
	import flash.geom.Point;
	import src.Constants;
	import src.Global;

	public class RoadPathFinder
	{
		private static function hasPoint(arr: Array, point: Point) : Boolean {

			var cnt: int = arr.length;

			for (var i: int = 0; i < cnt; i++) {
				var item: Point = arr[i];
				if (item.x == point.x && item.y == point.y) return true;
			}

			return false;
		}

		public static function isRoadByMapPosition(x: int, y: int) : Boolean {
			return isRoad(Global.map.regions.getTileAt(x, y));
		}

		public static function isRoad(tileId: int) : Boolean {
			return tileId >= Constants.road_start_tile_id && tileId <= Constants.road_end_tile_id;
		}

		public static function hasPath(start: Point, end: Point, city: City, excludedPoint: Point) : Boolean {

			if (start.x == end.x && start.y == end.y) return true;

			var visited: Array = new Array();

			visited.push(start);

			var fromStructure: Boolean = city.hasStructureAt(start);

			var ret: * = breadthFirst(new Point(end.x, end.y), visited, function(node : Point) : Array
			{
				var neighbors: Array = new Array();
				var possibleNeighbors: Array;
				if (node.y % 2 == 0)
				{
					possibleNeighbors = new Array(
					new Point(node.x, node.y - 1),
					new Point(node.x, node.y + 1),
					new Point(node.x - 1, node.y - 1),
					new Point(node.x - 1, node.y + 1)
					);
				}
				else
				{
					possibleNeighbors = new Array(
					new Point(node.x + 1, node.y - 1),
					new Point(node.x + 1, node.y + 1),
					new Point(node.x, node.y - 1),
					new Point(node.x, node.y + 1)
					);
				}

				for each (var location: Point in possibleNeighbors) {
					if ((location.x != end.x || location.y != end.y)) {
						if (hasPoint(visited, location)) continue;
						if (!isRoadByMapPosition(location.x, location.y)) continue;
						if (city.hasStructureAt(location)) continue;
						if (MapUtil.distance(location.x, location.y, city.MainBuilding.x, city.MainBuilding.y) > city.radius) continue;
					} else if (fromStructure && node.x == start.x && node.y == start.y) {
						continue;
					}

					neighbors.push(location);
				}

				return neighbors;
			}, excludedPoint);
			
			return ret;
		}

		private static function breadthFirst(end: Point, visited: Array, getNeighbors: Function, excludedPoint: Point, i: int = 0) : Boolean
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
				var aDist: Number = MapUtil.distance(a.x, a.y, end.x, end.y);
				var bDist: Number = MapUtil.distance(b.x, b.y, end.x, end.y);

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


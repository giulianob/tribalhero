package src.Map
{
	import flash.geom.Point;
	import src.Constants;
	import src.Global;
	import src.Util.BinaryList.BinaryList;

	public class RoadPathFinder
	{
		private static function hasPoint(arr: Array, point: Point) : Boolean {
			for each (var item: Point in arr) {
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
			var visited: Array = new Array();

			visited.push(start);

			return breadthFirst(new Point(end.x, end.y), visited, function(node : Point) : Array
			{
				var neighbors: Array = new Array(4);
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
					if (location.x != end.x || location.y != end.y) {
						if (city.hasStructureAt(location)) continue;
						if (!isRoadByMapPosition(location.x, location.y)) continue;
						if (MapUtil.distance(location.x, location.y, city.MainBuilding.x, city.MainBuilding.y) > city.radius) continue;
					}

					neighbors.push(location);
				}

				return neighbors;
			}, excludedPoint);
		}

		private static function breadthFirst(end: Point, visited: Array, getNeighbors: Function, excludedPoint: Point, i: int = 0) : Boolean
		{
			var nodes: Array = getNeighbors(visited[visited.length - 1]);

			// Examine adjacent nodes for end goal
			for each (var node: Point in nodes) {
				if ((node.x == end.x && node.y == end.y)) {
					return true;
				}
			}

			// Search nodes for goal
			for each (node in nodes) {
				if (!(node.x == end.x && node.y == end.y) && !(node.x == excludedPoint.x && node.y == excludedPoint.y) && !hasPoint(visited, node)) {
					visited.push(node);
					if (breadthFirst(end, visited, getNeighbors, excludedPoint, i)) return true;
					visited.pop();
				}
			}

			return false;
		}
	}

}


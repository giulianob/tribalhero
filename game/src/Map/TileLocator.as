/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.Map {

	import flash.geom.Point;
	import flash.geom.Rectangle;
	import src.Constants;
    import src.Map.ScreenPosition;
    import src.Util.Util;
	import src.Global;

	public class TileLocator {

		public static function getCityRegionId(rX: int, rY: int): int // from screen coord to region id
		{
			var xId: int = int(rX / Constants.cityRegionW);
			var yId: int = int(rY / int(Constants.cityRegionH / 2));

			if (xId < 0 || xId >= Constants.miniMapRegionW)
			return -1;
			if (yId < 0 || yId >= Constants.miniMapRegionH)
			return -1;

			var id: int = int(xId + yId * Constants.miniMapRegionW);

			if (Constants.debug >= 4)
			Util.log(rX + "," + rY + "(" + xId + "," + yId + ") =" + id);

			return id;
		}

		public static function getRegionIdFromMapCoord(rX: int, rY: int): int
		{
			var p: Point = getScreenCoord(rX, rY);
			return getRegionId(new ScreenPosition(p.x, p.y));
		}
		
		public static function getRegionRect(id: int): Rectangle
		{
			var x: int = (id % Constants.mapRegionW) * Constants.regionW;
			var y: int = int(id / Constants.mapRegionW) * (Constants.regionH / 2);			
			return new Rectangle(x, y, Constants.regionW, Constants.regionH / 2);
		}

		public static function getRegionId(screenPosition: ScreenPosition): int
		{
			var rX: int = screenPosition.x + Constants.tileW / 2;
			var rY: int = screenPosition.y + Constants.tileH / 2;

			var xId: int = int(rX / Constants.regionW);
			var yId: int = int(rY / int(Constants.regionH / 2));

			if (xId < 0 || xId >= Constants.mapRegionW)
			return -1;
			if (yId < 0 || yId >= Constants.mapRegionH)
			return -1;

			var id: int = int(xId + yId * Constants.mapRegionW);

			if (Constants.debug >= 4)
			Util.log(rX + "," + rY + "(" + xId + "," + yId + ") =" + id);

			return id;
		}

        public static function getMapCoord(x: int, y: int): Point // from screen coord to map coord
		{
			var xcoord: int = int(Math.max(x + int(Constants.tileW / 2), 0) / Constants.tileW);
			var ycoord: int = Math.round(Math.max(y + (Constants.tileH / 2), 0) / (Constants.tileH / 2));

			return new Point(xcoord, ycoord);
		}
		
		public static function getScreenMinimapToMapCoord(x: int, y: int): Point // from screen minimap coord (e.g. obj.x ) to the true map coord
		{
			var xcoord: int = int(Math.max(x + int(Constants.miniMapTileW / 2), 0) / Constants.miniMapTileW);
			var ycoord: int = Math.round(Math.max(y + (Constants.miniMapTileH / 2), 0) / (Constants.miniMapTileH / 2));

			return new Point(xcoord, ycoord);
		}

		public static function getScreenCoord(x: int, y: int) : Point // from map coord to where it belongs on screen
		{
			var xadd:int = (Math.abs(y % 2) == 1) ? (Constants.tileW / 2) : 0;

			var xcoord:int = int(x * Constants.tileW + xadd - int(Constants.tileW / 2));
			var ycoord:int = int(y * (Constants.tileH / 2) - (Constants.tileH / 2));

			return new Point(xcoord, ycoord);
		}

		public static function getMiniMapScreenCoord(x: int, y: int) : Point // from map coord to where it belongs on screen
		{
			var xadd:int = 0;

			if (Math.abs(y % 2) == 1) //odd tile
			xadd =+ (Constants.miniMapTileW / 2);

			var xcoord:int = Math.floor(x * Constants.miniMapTileW + xadd - int(Constants.miniMapTileW / 2));
			var ycoord:int = Math.floor(y * (Constants.miniMapTileH / 2) - (Constants.miniMapTileH / 2));

			return new Point(xcoord, ycoord);
		}

		public static function getActualCoord(x: int, y: int) : ScreenPosition //takes in screen coord and returns where obj belongs on screen (basically just rounds input screen pos to real screen pos)
		{
			y = Math.round(y / (Constants.tileH / 2));
			x = int((x + ( y % 2 == 0 ? Constants.tileW / 2 : 0)) / Constants.tileW);

			return getScreenPosition(x, y);
		}
		
		public static function getPointWithZoomFactor(x: int, y: int): Point //takes in coords (usually the stageX/stageY which are mouse coords and returns with zoom factor calculated)
		{
			return new Point(x * Global.gameContainer.camera.getZoomFactorOverOne(), y * Global.gameContainer.camera.getZoomFactorOverOne());
		}

		public static function regionXOffset(regionId: int): int
		{
			return (regionId % Constants.mapRegionW) * Constants.regionTileW;
		}

		public static function regionYOffset(regionId: int): int
		{
			return int(regionId / Constants.mapRegionW) * Constants.regionTileH;
		}

		public static function distance(x_1: int, y_1: int, x_2: int, y_2: int): int
		{
			var offset: int = 0;

			if (y_2 % 2 == 1 && y_1 % 2 == 0 && x_1 <= x_2) offset = 1;
			if (y_2 % 2 == 0 && y_1 % 2 == 1 && x_1 >= x_2) offset = 1;

			return ((x_1 > x_2 ? x_1 - x_2 : x_2 - x_1) + (y_1 > y_2 ? y_1 - y_2 : y_2 - y_1) / 2 + offset);
		}

		public static function radiusDistance(x: int, y: int, x1: int, y1: int) : int
		{
			if (x == x1 && y == y1) return 0;

			var offset: int = 0;
			var xoffset: int = 0;
			if (y%2 != y1%2) {
				if (y%2 == 0) {
					if (x > x1) {
						xoffset = 1;
					}
				} else {
					if (x1 > x) {
						xoffset = 1;
					}
				}
				offset = 1;
			}

			var xDistance: int = int(Math.abs(x - x1) - xoffset);
			var yDistance: int = int(Math.abs(y - y1));
			var yhalf: int = yDistance/2;
			var x05: int = int(Math.min(xDistance, yhalf));
			var x15: int = xDistance > yhalf ? xDistance - yhalf : 0;
			var radius: Number = x05 * 0.5 + x15 * 1.5 + yhalf * 1.5 + offset;

			return Math.max(0, int(radius * 2) - 1);
		}

        public static function foreachRadius(ox: int, oy: int, radius: int, do_self: Boolean = true):Array {
            var positions: Array = [];

            var tileRadius: int = Math.ceil(radius / 2.0);
            for each(var position: Position in foreachTile(ox, oy, tileRadius, do_self)) {
                if (radiusDistance(ox, oy, position.x, position.y) <= radius) {
                    positions.push(position);
                }
            }

            return positions;
        }

		public static function foreachTile(ox: int, oy: int, radius: int, do_self: Boolean = true): Array {
            var positions: Array = [];

			var mode: int;
			if (ox % 2 == 0) {
				if (oy % 2 == 0) {
					mode = 0;
				} else {
					mode = 1;
				}
			} else {
				if (oy % 2 == 0) {
					mode = 0;
				} else {
					mode = 1;
				}
			}

			var cx: int = ox;
			var cy: int = oy - (2 * radius);
			var last: int = cx;
			for (var row: int = 0; row < radius * 2 + 1; ++row) {
				for (var count: int = 0; count < radius * 2 + 1; ++count) {

					if ((row % 2 == 0 && mode == 0) || (row % 2 != 0 && mode == 1)) {
						if (!do_self && ox == cx + (count / 2) && oy == cy + count) continue;
						positions.push(new Position(cx + (count / 2), cy + count));
					} else {
						if (!do_self && ox == cx + ((count+1) / 2) && oy == cy + count) continue;
                        positions.push(new Position(cx + ((count + 1) / 2), cy + count));
					}

				}

				if (mode == 0) {
					cx -= ((row+1) % 2);
				} else {
					cx -= ((row) % 2);
				}

				++cy;
			}

            return positions;
		}


        public function foreachMultitile(ox: int, oy: int, size: int): Array {
            var positions: Array = [];

            var position: Position = new Position(ox, oy);
            for (var i: int = 0; i < size; i++)
            {
                var rowPosition: Position = position;
                for (var j: int = 0; j < size; j++)
                {
                    positions.push(rowPosition);

                    rowPosition = rowPosition.topRight();
                }

                position = position.bottomRight();
            }

            return positions;
        }

        public static function getMapPosition(x: Number, y: Number): Position {
            var pt: Point = getMapCoord(x, y);
            return new Position(pt.x, pt.y);
        }

        public static function getScreenPosition(x: Number, y: Number): ScreenPosition {
            var pt: Point = getScreenCoord(x, y);
            return new ScreenPosition(pt.x, pt.y);
        }
    }
}


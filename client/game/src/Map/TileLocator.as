/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.Map {

    import System.Linq.Enumerable;

    import flash.geom.Point;
    import flash.geom.Rectangle;

    import src.Constants;
    import src.Global;
    import src.Objects.SimpleObject;
    import src.Util.Util;

    public class TileLocator {

		public static function getMiniMapRegionId(rX: int, rY: int): int // from screen coord to region id
		{
			var xId: int = int(rX / Constants.miniMapRegionW);
			var yId: int = int(rY / int(Constants.miniMapRegionH / 2));

			if (xId < 0 || xId >= Constants.miniMapRegionRatioW)
			return -1;
			if (yId < 0 || yId >= Constants.miniMapRegionRatioH)
			return -1;

			var id: int = int(xId + yId * Constants.miniMapRegionRatioW);

			if (Constants.debug >= 4)
			Util.log(rX + "," + rY + "(" + xId + "," + yId + ") =" + id);

			return id;
		}

		public static function getRegionIdFromMapCoord(position: Position): int
		{
            return int(position.x / Constants.regionTileW) + int(position.y / Constants.regionTileH) * int(Constants.mapTileW / Constants.regionTileW);
		}
		
		public static function getRegionRect(id: int): Rectangle
		{
			var x: int = (id % Constants.mapRegionW) * Constants.regionW;
			var y: int = int(id / Constants.mapRegionW) * (Constants.regionH / 2);			
			return new Rectangle(x, y, Constants.regionW, Constants.regionH / 2);
		}

        public static function getTileIndex(position: Position): int
        {
            return (int)(position.x % Constants.regionTileW + (position.y % Constants.regionTileH) * Constants.regionTileW);
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

			return id;
		}

        public static function getMapCoord(screenPos: ScreenPosition): Position // from screen coord to map coord
		{
			var xcoord: int = int(Math.max(screenPos.x + int(Constants.tileW / 2), 0) / Constants.tileW);
			var ycoord: int = Math.round(Math.max(screenPos.y + (Constants.tileH / 2), 0) / (Constants.tileH / 2));

			return new Position(xcoord, ycoord);
		}
		
		public static function getScreenMinimapToMapCoord(x: int, y: int): Position // from screen minimap coord (e.g. obj.x ) to the true map coord
		{
			var xcoord: int = int(Math.max(x + int(Constants.miniMapTileW / 2), 0) / Constants.miniMapTileW);
			var ycoord: int = Math.round(Math.max(y + (Constants.miniMapTileH / 2), 0) / (Constants.miniMapTileH / 2));

			return new Position(xcoord, ycoord);
		}

		public static function getScreenCoord(position: Position) : ScreenPosition // from map coord to where it belongs on screen
		{
			var xadd:int = (Math.abs(position.y % 2) == 1) ? (Constants.tileW / 2) : 0;

			var xcoord:int = int(position.x * Constants.tileW + xadd - int(Constants.tileW / 2));
			var ycoord:int = int(position.y * (Constants.tileH / 2) - (Constants.tileH / 2));

			return new ScreenPosition(xcoord, ycoord);
		}

		public static function getMiniMapScreenCoord(x: int, y: int) : ScreenPosition // from map coord to where it belongs on screen
		{
			var xadd:int = 0;

			if (Math.abs(y % 2) == 1) //odd tile
			xadd =+ (Constants.miniMapTileW / 2);

			var xcoord:int = Math.floor(x * Constants.miniMapTileW + xadd - int(Constants.miniMapTileW / 2));
			var ycoord:int = Math.floor(y * (Constants.miniMapTileH / 2) - (Constants.miniMapTileH / 2));

			return new ScreenPosition(xcoord, ycoord);
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

        public static function containsIntersectingPoints(positions1: Array, positions2: Array): Boolean {
            for each (var position1: Position in positions1) {
                for each (var position2: Position in positions2) {
                    if (position1.equals(position2)) {
                        return true;
                    }
                }
            }

            return false;
        }

		public static function distance(x: int, y: int, size: int, x2: int, y2: int, size2: int): int
		{
            var min: int = int.MAX_VALUE;
            for each (var position: Position in foreachMultitile(x, y, size))
            {
                for each (var position1: Position in foreachMultitile(x2, y2, size2)) {
                    min = Math.min(min, distanceSingle(position.x, position.y, position1.x, position1.y));
                }
            }

            return min;
        }

        private static function distanceSingle(x1: int, y1: int, x2: int, y2: int): int
        {
			var offset: int = 0;

			if (y2 % 2 == 1 && y1 % 2 == 0 && x1 <= x2) offset = 1;
			if (y2 % 2 == 0 && y1 % 2 == 1 && x1 >= x2) offset = 1;

			return ((x1 > x2 ? x1 - x2 : x2 - x1) + (y1 > y2 ? y1 - y2 : y2 - y1) / 2 + offset);
		}

        public static function radiusDistance(x: int, y: int, size: int, x2: int, y2: int, size2: int): int
        {
            var min: int = int.MAX_VALUE;
            for each (var position: Position in foreachMultitile(x, y, size))
            {
                for each (var position1: Position in foreachMultitile(x2, y2, size2)) {
                    min = Math.min(min, radiusDistanceSingle(position.x, position.y, position1.x, position1.y));
                }
            }

            return min;
        }

		private static function radiusDistanceSingle(x: int, y: int, x1: int, y1: int) : int
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
                if (radiusDistanceSingle(ox, oy, position.x, position.y) <= radius) {
                    positions.push(position);
                }
            }

            return positions;
        }

        public static function foreachRadiusWithSize(ox: int, oy: int, size: int, radius: int): Array {
            var tilePositions: Array = foreachMultitile(ox, oy, size);
            var positionComparer: PositionComparer = new PositionComparer();

            return Enumerable.from(tilePositions).selectMany(function (position: Position): Array {
                return foreachRadius(position.x, position.y, radius, false);
            }).where(function (position: Position): Boolean {
                return !Enumerable.from(tilePositions).contains(position, positionComparer);
            }).toArray();
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

        public static function foreachMultitileObject(obj: SimpleObject): Array {
            var mapPosition: Position = obj.primaryPosition.toPosition();

            return foreachMultitile(mapPosition.x, mapPosition.y, obj.size);
        }

        public static function foreachMultitile(ox: int, oy: int, size: int): Array {
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

        public static function getScreenPosition(x: Number, y: Number): ScreenPosition {
            return getScreenCoord(new Position(x, y));
        }
    }
}


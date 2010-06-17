package src.Objects
{
	import flash.geom.Point;
	import src.Global;
	import src.Map.Map;
	import src.Map.MapUtil;
	import src.Objects.Factories.ObjectFactory;

	public class WallManager
	{
		private static const WALL_WIDTH: int = 11;

		private static const WALL_HEIGHT: int = 24;

		private static const WALLS: Array = [
		[
		["",   "",   "",   "",   "",   "",   "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "",   "",   "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "",   "",   "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "",   "",   "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "",   "",   "",   "",   "",   "",   "",   ],
		["",   "",   "",   "SE", "O4", "O3", "",   "",   "",   "",   "",   ],
		["",   "",   "",   "SE", "NW", "",   "NE", "",   "",   "",   "",   ],
		["",   "",   "SE", "",   "",   "",   "NE", "",   "",   "",   "",   ],
		["",   "",   "O3", "",   "",   "",   "",   "O2", "",   "",   "",   ],
		["",   "NW", "",   "",   "",   "",   "",   "SW", "",   "",   "",   ],
		["",   "NW", "",   "",   "",   "",   "",   "",   "O3", "",   "",   ],
		["NW", "",   "",   "",   "",   "",   "",   "",   "NE", "",   "",   ],
		["O2", "",   "",   "",   "",   "",   "",   "",   "",   "NE", "",   ],
		["SW", "",   "",   "",   "",   "",   "",   "",   "",   "E", "",   ],
		["",   "O3", "",   "",   "",   "",   "",   "",   "",   "SE", "",   ],
		["",   "NE", "",   "",   "",   "",   "",   "",   "O3", "",   "",   ],
		["",   "",   "NE", "",   "",   "",   "",   "",   "NW", "",   "",   ],
		["",   "",   "O2", "",   "",   "",   "",   "NW", "",   "",   "",   ],
		["",   "",   "",   "SW", "",   "",   "",   "O1", "",   "",   "",   ],
		["",   "",   "",   "O3", "",   "",   "SE", "",   "",   "",   "",   ],
		["",   "",   "",   "",   "O2", "",   "SE", "",   "",   "",   "",   ],
		["",   "",   "",   "",   "SW", "SE", "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "",   "S", "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "",   "",   "",   "",   "",   "",   "",   ],
		],
		[
		["",   "",   "",   "",   "",   "",   "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "",   "",   "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "",   "",   "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "",   "",   "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "",   "N", "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "NW", "NE", "",   "",   "",   "",   "",   ],
		["",   "",   "",   "SE", "NW", "",   "NE", "",   "",   "",   "",   ],
		["",   "",   "O4", "",   "",   "",   "NE", "",   "",   "",   "",   ],
		["",   "",   "NW", "",   "",   "",   "",   "O2", "",   "",   "",   ],
		["",   "W", "",   "",   "",   "",   "",   "SW", "",   "",   "",   ],
		["",   "",   "",   "",   "",   "",   "",   "",   "O3", "",   "",   ],
		["N", "",   "",   "",   "",   "",   "",   "",   "NE", "",   "",   ],
		["W", "",   "",   "",   "",   "",   "",   "",   "",   "NE", "",   ],
		["O3", "",   "",   "",   "",   "",   "",   "",   "",   "E", "",   ],
		["",   "NE", "",   "",   "",   "",   "",   "",   "",   "O4", "",   ],
		["",   "NE", "",   "",   "",   "",   "",   "",   "O1", "",   "",   ],
		["",   "",   "NE", "",   "",   "",   "",   "",   "SE", "",   "",   ],
		["",   "",   "NE", "",   "",   "",   "",   "SE", "",   "",   "",   ],
		["",   "",   "",   "O2", "",   "",   "",   "SE", "",   "",   "",   ],
		["",   "",   "",   "O3", "",   "SE", "O3", "",   "",   "",   "",   ],
		["",   "",   "",   "",   "O2", "SE", "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "S", "",   "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "",   "",   "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "",   "",   "",   "",   "",   "",   "",   ],
		],
		[
		["",   "",   "",   "",   "",   "",   "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "",   "",   "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "",   "",   "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "",   "",   "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "SE", "N", "",   "",   "",   "",   "",   ],
		["",   "",   "",   "SE", "NW", "NE", "",   "",   "",   "",   "",   ],
		["",   "",   "",   "O4", "",   "",   "NE", "",   "",   "",   "",   ],
		["",   "",   "O1", "",   "",   "",   "NE", "",   "",   "",   "",   ],
		["",   "",   "O3", "",   "",   "",   "",   "NE", "",   "",   "",   ],
		["",   "NW", "",   "",   "",   "",   "",   "NE", "",   "",   "",   ],
		["",   "NW", "",   "",   "",   "",   "",   "",   "O2", "",   "",   ],
		["NW", "",   "",   "",   "",   "",   "",   "",   "SW", "",   "",   ],
		["W", "",   "",   "",   "",   "",   "",   "",   "",   "O3", "",   ],
		["SW", "",   "",   "",   "",   "",   "",   "",   "",   "E", "",   ],
		["",   "SW", "",   "",   "",   "",   "",   "",   "",   "",   "",   ],
		["",   "SW", "",   "",   "",   "",   "",   "",   "N", "",   "",   ],
		["",   "",   "",   "",   "",   "",   "",   "",   "NW", "NE", "",   ],
		["",   "W", "",   "",   "",   "",   "",   "O1", "",   "",   "",   ],
		["",   "",   "O3", "",   "",   "",   "",   "SE", "",   "",   "",   ],
		["",   "",   "NE", "",   "",   "",   "SE", "",   "",   "",   "",   ],
		["",   "",   "",   "NE", "O4", "SW", "SE", "",   "",   "",   "",   ],
		["",   "",   "",   "",   "",   "O3", "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "",   "",   "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "",   "",   "",   "",   "",   "",   "",   ],
		],
		[
		["",   "",   "",   "",   "",   "",   "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "",   "",   "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "",   "",   "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "",   "",   "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "",   "N", "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "NW", "E", "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "NW", "",   "",   "W", "",   "",   "",   ],
		["",   "",   "",   "O1", "",   "",   "",   "SW", "",   "",   "",   ],
		["",   "",   "",   "SE", "",   "",   "",   "",   "SW", "",   "",   ],
		["",   "",   "SE", "",   "",   "",   "",   "",   "SW", "",   "",   ],
		["",   "",   "O3", "",   "",   "",   "",   "",   "",   "O3", "",   ],
		["",   "NW", "",   "",   "",   "",   "",   "",   "",   "E", "",   ],
		["",   "NW", "",   "",   "",   "",   "",   "",   "",   "O4", "",   ],
		["NW", "",   "",   "",   "",   "",   "",   "",   "NW", "",   "",   ],
		["W", "",   "",   "",   "",   "",   "",   "",   "O1", "",   "",   ],
		["SW", "",   "",   "",   "",   "",   "",   "SE", "",   "",   "",   ],
		["",   "SW", "",   "",   "",   "",   "",   "SE", "",   "",   "",   ],
		["",   "SW", "",   "",   "",   "",   "SE", "",   "",   "",   "",   ],
		["",   "",   "SW", "",   "",   "",   "SE", "",   "",   "",   "",   ],
		["",   "",   "SW", "O4", "SW", "SE", "",   "",   "",   "",   "",   ],
		["",   "",   "",   "NW", "",   "O3", "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "",   "",   "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "",   "",   "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "",   "",   "",   "",   "",   "",   "",   ],
		],
		[
		["",   "",   "",   "",   "",   "",   "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "",   "",   "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "",   "",   "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "",   "",   "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "",   "N", "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "O1", "NE", "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "SE", "",   "NE", "",   "",   "",   "",   ],
		["",   "",   "",   "O4", "",   "",   "O2", "",   "",   "",   "",   ],
		["",   "",   "",   "NW", "",   "",   "",   "O3", "",   "",   "",   ],
		["",   "",   "NW", "",   "",   "",   "",   "NE", "",   "",   "",   ],
		["",   "",   "NW", "",   "",   "",   "",   "",   "NE", "",   "",   ],
		["",   "O1", "",   "",   "",   "",   "",   "",   "NE", "",   "",   ],
		["",   "SE", "",   "",   "",   "",   "",   "",   "",   "E", "",   ],
		["",   "",   "",   "",   "",   "",   "",   "",   "NW", "",   "",   ],
		["",   "NE", "",   "",   "",   "",   "",   "",   "O1", "",   "",   ],
		["",   "NE", "",   "",   "",   "",   "",   "SE", "",   "",   "",   ],
		["",   "",   "O2", "",   "",   "",   "",   "SE", "",   "",   "",   ],
		["",   "",   "SW", "",   "",   "",   "SE", "",   "",   "",   "",   ],
		["",   "",   "",   "SW", "",   "",   "SE", "",   "",   "",   "",   ],
		["",   "",   "",   "O3", "",   "SE", "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "",   "S", "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "",   "",   "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "",   "",   "",   "",   "",   "",   "",   ],
		["",   "",   "",   "",   "",   "",   "",   "",   "",   "",   "",   ],
		],
		];

		private static const WALL_VARIATIONS: int = 2;

		public var objects: Array = new Array();

		private var map: Map;
		private var parent: SimpleGameObject;
		public var radius: int = 0;

		public function WallManager(map: Map, parent: SimpleGameObject)
		{
			this.map = map;
			this.parent = parent;
		}

		public function clear():void {
			for each(var obj: IScrollableObject in objects) {
				map.objContainer.removeObject(obj);
			}

			objects = new Array();
		}

		public function draw(radius: int):void {
			this.radius = radius;
			clear();
			var pos: Point = MapUtil.getMapCoord(parent.getX(), parent.getY());

			var typeHash: int = wallTypeHash(pos.x, pos.y);

			for (var y: int = 0; y < WALL_HEIGHT; y++) {
				for (var x: int = 0; x < WALL_WIDTH; x++) {
					if (WALLS[typeHash][y][x] == "") continue;

					var mapX: int = pos.x + ( x - int(WALL_WIDTH/2));
					var mapY: int = pos.y + ( y - int(WALL_HEIGHT/2));

					pushWall(WALLS[typeHash][y][x], mapX, mapY);
				}
			}
		}

		public function addWallCallback(x: int, y: int, custom: *):void
		{
			var parentPos: Point = MapUtil.getMapCoord(parent.getX(), parent.getY());

			var dist: int = MapUtil.distance(parentPos.x, parentPos.y, x, y);

			if (dist != custom) {
				return;
			}

			var wall: String = "";

			if (parentPos.y % 2 == 0)
			{
				if (x < parentPos.x && y < parentPos.y)
				wall = "WALL_NW";
				else if (x < parentPos.x && y > parentPos.y)
				wall = "WALL_SW";
				else if ((x > parentPos.x && y > parentPos.y) || (x== parentPos.x && y%2==1 && y > parentPos.y))
				wall = "WALL_SE";
				else if ((x > parentPos.x && y < parentPos.y) || (x == parentPos.x && y%2==1 && y < parentPos.y))
				wall = "WALL_NE";
			}
			else
			{
				if ((x < parentPos.x && y < parentPos.y) || (x == parentPos.x && y%2==0 && y < parentPos.y))
				wall = "WALL_NW";
				else if (x < parentPos.x && y > parentPos.y || (x == parentPos.x && y%2==0 && y > parentPos.y))
				wall = "WALL_SW";
				else if (x > parentPos.x && y > parentPos.y)
				wall = "WALL_SE";
				else if (x > parentPos.x && y < parentPos.y || (parentPos.y%2 == 0 && x == parentPos.x && y < parentPos.y))
				wall = "WALL_NE";
			}

			if (wall == "")
			return;

			pushWall(wall, x, y);
		}

		private function wallTypeHash(x: int, y: int) : int {
			return Math.max(0, ((x * parent.cityId * 0x1f1f1f1f) ^ y) % WALLS.length);
		}

		private function wallHash(x: int, y: int) : int {
			return Math.max(0, ((x * parent.cityId * 0x1f1f1f1f) ^ y) % WALL_VARIATIONS);
		}

		private function pushWall(wallName: String, x: int, y: int) : void {

			var wall: SimpleObject = ObjectFactory.getSimpleObject("WALL_" + wallName + (wallName.charAt(0) == 'O' ? "" : "_" + wallHash(x, y).toString()));

			var pos: Point = MapUtil.getScreenCoord(x, y);
			x = pos.x;
			y = pos.y;

			wall.setX(x);
			wall.setY(y);
			map.objContainer.addObject(wall);
			wall.moveWithCamera(Global.gameContainer.camera);

			objects.push(wall);
		}
	}

}


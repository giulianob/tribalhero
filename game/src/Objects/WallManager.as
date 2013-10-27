package src.Objects
{
    import flash.display.Bitmap;
    import flash.display.DisplayObject;
    import flash.events.Event;

    import src.Constants;
    import src.Global;
    import src.Map.Position;
    import src.Map.ScreenPosition;
    import src.Objects.Factories.StrongholdFactory;
    import src.Objects.Factories.TroopFactory;
    import src.Objects.States.GameObjectState;

    public class WallManager
	{
		private static const WALL_WIDTH: int = 15;

		private static const WALL_HEIGHT: int = 29;

		private static const WALLS: Array = [
		// Radius 0
		[],
		// Radius 1
		[],
		// Radius 2		
        [],
		// Radius 3
		[],
		// Radius 4
		[],
		// Radius 5
            [
                [
                    ["", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	],
                    ["", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	],
                    ["", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	],
                    ["", 	"", 	"", 	"", 	"", 	"", 	"5", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	],
                    ["", 	"", 	"", 	"", 	"", 	"", 	"10", 	"9", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	],
                    ["", 	"", 	"", 	"", 	"", 	"1", 	"", 	"8", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	],
                    ["", 	"", 	"", 	"", 	"", 	"1", 	"", 	"", 	"8", 	"", 	"", 	"", 	"", 	"", 	"", 	],
                    ["", 	"", 	"", 	"", 	"1", 	"", 	"", 	"", 	"8", 	"", 	"", 	"", 	"", 	"", 	"", 	],
                    ["", 	"", 	"", 	"", 	"1", 	"", 	"", 	"", 	"", 	"8", 	"", 	"", 	"", 	"", 	"", 	],
                    ["", 	"", 	"", 	"1", 	"", 	"", 	"", 	"", 	"", 	"8", 	"", 	"", 	"", 	"", 	"", 	],
                    ["", 	"", 	"", 	"1", 	"", 	"", 	"", 	"", 	"", 	"", 	"8", 	"", 	"", 	"", 	"", 	],
                    ["", 	"", 	"1", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"8", 	"", 	"", 	"", 	"", 	],
                    ["", 	"", 	"1", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"8", 	"", 	"", 	"", 	],
                    ["", 	"2", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"8", 	"", 	"", 	"", 	],
                    ["", 	"", 	"8", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"8", 	"", 	"", 	],
                    ["", 	"", 	"8", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"6", 	"", 	"", 	],
                    ["", 	"", 	"", 	"6", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"1", 	"", 	"", 	],
                    ["", 	"", 	"2", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"1", 	"", 	"", 	"", 	],
                    ["", 	"", 	"", 	"8", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"1", 	"", 	"", 	"", 	],
                    ["", 	"", 	"", 	"8", 	"", 	"", 	"", 	"", 	"", 	"", 	"1", 	"", 	"", 	"", 	"", 	],
                    ["", 	"", 	"", 	"", 	"8", 	"", 	"", 	"", 	"", 	"", 	"1", 	"", 	"", 	"", 	"", 	],
                    ["", 	"", 	"", 	"", 	"8", 	"", 	"", 	"", 	"", 	"1", 	"", 	"", 	"", 	"", 	"", 	],
                    ["", 	"", 	"", 	"", 	"", 	"8", 	"", 	"", 	"5", 	"1", 	"", 	"", 	"", 	"", 	"", 	],
                    ["", 	"", 	"", 	"", 	"", 	"8", 	"5", 	"10", 	"7", 	"", 	"", 	"", 	"", 	"", 	"", 	],
                    ["", 	"", 	"", 	"", 	"", 	"", 	"4", 	"7", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	],
                    ["", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	],
                    ["", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	],
                    ["", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	],
                    ["", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	"", 	],
                ],
            ]
        ];

        
        // Controls how many different wall styles we have for the same wall tile (e.g. one w/ vines, one w/o vines)
		private static const WALL_VARIATIONS: int = 2;

		public var objects: Array = [];
        public var bitmaps: Array = [];

		private var parent: SimpleGameObject;
		public var radius: int = 0;

		public function WallManager(parent: SimpleGameObject, radius: int)
		{
			this.parent = parent;			
			this.radius = radius;
			
			draw(radius);
		}
		
		private function onAddedToStage(e: Event): void {
			draw(radius);
		}

		public function clear():void {
			parent.removeEventListener(Event.ADDED_TO_STAGE, onAddedToStage);
			
			for each(var obj: DisplayObject in objects) {
				Global.map.objContainer.removeObject(obj);
            }

			objects = [];

            for each(var bitmap: Bitmap in bitmaps) {
                bitmap.bitmapData.dispose();
            }

            bitmaps = [];
		}

		public function draw(radius: int):void {						
			this.radius = radius;

			clear();			

			if (radius == 0 || radius >= WALLS.length || WALLS[radius].length == 0) return;
			
			// Delay until the obj is in the stage
			if (parent.stage == null) {
				parent.addEventListener(Event.ADDED_TO_STAGE, onAddedToStage, false, 0, true);
				return;
			}

            var pos: Position = parent.primaryPosition.toPosition();

            // For city wqlls just manually center it.. It's a bit ugly but we dont
            // have the city info for the SImpleGameObject so we cant easily get the real city x/y at the moment
            if (parent.size == 3) {
                pos = pos.right();
            }

			var typeHash: int = wallTypeHash(pos.x, pos.y, radius);

			for (var y: int = 0; y < WALL_HEIGHT; y++) {
				for (var x: int = 0; x < WALL_WIDTH; x++) {
					if (WALLS[radius][typeHash][y][x] === "") continue;

					var mapX: int = pos.x + ( x - int(WALL_WIDTH/2));
					var mapY: int = pos.y + ( y - int(WALL_HEIGHT / 2));

					if (pos.y % 2 == 1 && y % 2 == 1) mapX += 1;

					pushWall(int(WALLS[radius][typeHash][y][x]), mapX, mapY);
				}
			}
		}

		private function wallTypeHash(x: int, y: int, wallIdx: int) : int {
			return Math.max(0, ((x * parent.groupId * 0x1f1f1f1f) ^ y) % WALLS[wallIdx].length);
		}

		private function wallHash(x: int, y: int) : int {
			return Math.max(0, ((x * parent.groupId * 0x1f1f1f1f) ^ y) % WALL_VARIATIONS);
		}

		private function pushWall(tileId: int, x: int, y: int) : void {
            // TODO: Add wall variations back in
            var wall: Bitmap = Constants.wallTileset.getTile(tileId);

            var mapPosition: Position = new Position(x, y);
            var screenPosition: ScreenPosition = mapPosition.toScreenPosition();

            var simpleObject: SimpleObject = new SimpleObject(screenPosition.x, screenPosition.y, 1);
            simpleObject.addChild(wall);

			Global.map.objContainer.addObject(simpleObject);

            bitmaps.push(wall);
			objects.push(simpleObject);
		}
	}

}


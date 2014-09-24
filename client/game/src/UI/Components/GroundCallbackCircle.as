package src.UI.Components {
    import flash.geom.Point;

    import src.Constants;
    import src.Global;
    import src.Map.Position;
    import src.Map.ScreenPosition;
    import src.Map.TileLocator;
    import src.Objects.Factories.SpriteFactory;
    import src.Objects.ObjectContainer;
    import src.Objects.SimpleObject;

    public class GroundCallbackCircle
	{
		private var callback: Function;
        private var _size: int;
        private var tiles: Array = [];
        protected var mainPosition: ScreenPosition;
        private var skipCenter: Boolean;

		public function GroundCallbackCircle(size: int, mainPosition: ScreenPosition, callback: Function, skipCenter: Boolean = false) {
			this._size = size;
            this.mainPosition = mainPosition;
            this.callback = callback;
            this.skipCenter = skipCenter;
        }

        public function moveTo(mainPosition: ScreenPosition): void {
            this.mainPosition = mainPosition;
            draw();
        }

		public function draw() : void {
            clear();

			for each (var position: Position in TileLocator.foreachTile(_size, _size * 2 + 1, _size, !skipCenter)) {
                var tile: SimpleObject = new SimpleObject(0, 0, 1);
                tile.setSprite(SpriteFactory.getStarlingImage("MASK_TILE"), new Point(0, 0));

                var point: ScreenPosition = position.toScreenPosition();
                var x: Number = mainPosition.x + point.x - _size * Constants.tileW;
                var y: Number = mainPosition.y + point.y - (_size * Constants.tileH);
                var colorTransform: * = callback(x, y);

                if (colorTransform === false) {
                    continue;
                }

                tile.x = tile.primaryPosition.x = x;
                tile.y = tile.primaryPosition.y = y;
                tile.sprite.color = colorTransform;
                tile.alpha = 0.7;

                Global.map.objContainer.addObject(tile, ObjectContainer.LOWER);
                tiles.push(tile);
            }
		}

		public function dispose():void
		{
			clear();
		}

        public function clear(): void {
            for each (var tile: SimpleObject in tiles) {
                Global.map.objContainer.removeObject(tile, ObjectContainer.LOWER);
            }

            tiles = [];
        }

        public function get visible(): Boolean {
            return tiles.length > 0;
        }

        public function get size(): int {
            return _size;
        }
    }

}


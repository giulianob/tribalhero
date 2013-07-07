package src.UI.Components {
    import flash.display.Bitmap;
    import flash.display.BitmapData;
    import flash.display.DisplayObject;
    import flash.display.DisplayObjectContainer;
    import flash.display.MovieClip;
    import flash.events.Event;
    import flash.geom.ColorTransform;
    import flash.geom.Point;

    import src.Constants;
    import src.Assets;
    import src.Map.Position;
    import src.Map.TileLocator;
    import src.Objects.SimpleObject;

    public class GroundCircle extends SimpleObject
	{
		private var size: int;
		private var circle: DisplayObjectContainer;
		private var tiles: Array;
		private var colorTransform: ColorTransform;
		private var skipCenter: Boolean;

		public function GroundCircle(size: int, skipCenter: Boolean = false, colorTransform: ColorTransform = null) {
			super( -10, -10);
			
			if (colorTransform == null) {
				colorTransform = new ColorTransform(1.0, 1.0, 1.0, 1.0, 0, 100, 0);
			}

			this.skipCenter = skipCenter;
			this.colorTransform = colorTransform;
			this.size = size;
			drawCircle(this.size);
			addEventListener(Event.REMOVED_FROM_STAGE, onRemovedFromStage);
			addEventListener(Event.ADDED_TO_STAGE, onAddedToStage);
		}

		private function onAddedToStage(e: Event):void
		{
			drawCircle(size);
		}

		public function onRemovedFromStage(e: Event):void
		{
			dispose();
		}

		private function drawCircle(size: int):void
		{
			this.size = size;

			if (circle != null)
			dispose();

			circle = new MovieClip();
			tiles = [];

			for each (var position: Position in TileLocator.foreachRadius(Math.ceil(size / 2.0), Math.ceil(size / 2.0) * 2 + 1, size)) {
                var tiledata: DisplayObject = Assets.getInstance("MASK_TILE");
                var tile: Bitmap = new Bitmap(new BitmapData(Constants.tileW, Constants.tileH, true, 0x000000));
                tile.smoothing = true;

                var tileRadius: int = Math.ceil(size / 2.0);
                var point: Point = TileLocator.getScreenCoord(position.x, position.y);
                tile.x = point.x - tileRadius * Constants.tileW;
                tile.y = point.y - tileRadius * Constants.tileH;

                if (tile.x == 0 && tile.y == 0 && skipCenter) {
                    continue;
                }

                tile.bitmapData.draw(tiledata, null, colorTransform);

                circle.addChild(tile);

                tiles.push(tile);
            }

			addChild(circle);
		}

		override public function dispose():void 
		{
			super.dispose();

			if (circle)
			{
				removeChild(circle);
				circle = null;
			}

			if (tiles)
			{
				for each(var tile: Bitmap in tiles)
				tile.bitmapData.dispose();

				tiles = null;
			}
		}
	}

}


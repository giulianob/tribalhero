package src.UI.Components {
    import flash.display.Bitmap;
    import flash.display.BitmapData;
    import flash.display.DisplayObject;
    import flash.display.DisplayObjectContainer;
    import flash.display.MovieClip;
    import flash.events.Event;

    import src.Assets;
    import src.Constants;
    import src.Map.Position;
    import src.Map.ScreenPosition;
    import src.Map.TileLocator;
    import src.Objects.SimpleObject;

    public class GroundCallbackCircle extends SimpleObject
	{
		private var circle: DisplayObjectContainer;
		private var tiles: Array;
		private var callback: Function;

		public function GroundCallbackCircle(size: int, callback: Function) {
			super( -10, -10, size);
			
			this.size = size;
			this.callback = callback;
			addEventListener(Event.REMOVED_FROM_STAGE, onRemovedFromStage);
			addEventListener(Event.ADDED_TO_STAGE, onAddedToStage);
		}

		public function redraw() : void {
			drawCircle(size);
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
			for each (var position: Position in TileLocator.foreachTile(size, size * 2 + 1, size)) {
                var tiledata: DisplayObject = Assets.getInstance("MASK_TILE");
                var tile: Bitmap = new Bitmap(new BitmapData(Constants.tileW, Constants.tileH, true, 0x000000));
                tile.smoothing = true;
                var point: ScreenPosition = position.toScreenPosition();
                tile.x = point.x - size * Constants.tileW;
                tile.y = point.y - (size * Constants.tileH);
                var colorTransform: * = callback(tile.x, tile.y);
                if (colorTransform == false) {
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


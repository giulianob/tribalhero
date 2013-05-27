package src.UI.Components {
import flash.display.Bitmap;
import flash.display.BitmapData;
import flash.display.DisplayObject;
import flash.events.Event;
import flash.geom.Point;

import src.Constants;
import src.ImportObjects;
import src.Map.MapUtil;
import src.Objects.SimpleObject;
import src.UI.SmartMovieClip;

public class GroundCallbackCircle extends SimpleObject
	{
		private var size: int;
		private var circle: SmartMovieClip;
		private var tiles: Array;
		private var callback: Function;

		public function GroundCallbackCircle(size: int, callback: Function) {
			super( -10, -10);
			
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

			circle = new SmartMovieClip();
			tiles = new Array();
			MapUtil.foreach_object(size, size * 2 + 1, size, this.addTileCallback, true, null);
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

		public function addTileCallback(x: int, y: int, custom: *):void
		{
			var tiledata: DisplayObject = ImportObjects.getInstance("MASK_TILE");

			var tile: Bitmap = new Bitmap(new BitmapData(Constants.tileW, Constants.tileH, true, 0x000000));
			tile.smoothing = true;

			var point: Point = MapUtil.getScreenCoord(x, y);

			tile.x = point.x - size * Constants.tileW;
			tile.y = point.y - (size * Constants.tileH);

			var colorTransform: * = callback(tile.x, tile.y, tile.x == 0 && tile.y == 0);

			if (colorTransform == false) return;

			tile.bitmapData.draw(tiledata, null, colorTransform);

			circle.addChild(tile);

			tiles.push(tile);
		}

	}

}


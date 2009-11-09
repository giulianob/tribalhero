package src.UI.Components {
	import flash.display.Bitmap;
	import flash.display.BitmapData;
	import flash.events.Event;
	import flash.geom.ColorTransform;
	import flash.geom.Point;
	import src.Constants;
	import src.Map.MapUtil;
	import src.Objects.IScrollableObject;
	import src.Objects.SimpleObject;
	import src.UI.SmartMovieClip;
	/**
	* ...
	* @author Default
	*/
	public class GroundCircle extends SimpleObject
	{		
		private var size: int;
		private var circle: SmartMovieClip;
		private var tiles: Array;
		
		public function GroundCircle(size: int) {
			this.size = Math.min(size, 10);
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
			
			circle = new SmartMovieClip();	
			tiles = new Array();
			
			MapUtil.foreach_object(size, size * 2 + 1, size, this.addTileCallback, true, null);
			
			addChild(circle);
		}
		
		public function dispose():void
		{
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
			var tiledata: MASK_TILE = new MASK_TILE(Constants.tileW, Constants.tileH);
			
			var tile: Bitmap = new Bitmap(new BitmapData(Constants.tileW, Constants.tileH, true, 0x000000));			
				
			var point: Point = MapUtil.getScreenCoord(x, y);
			
			tile.x = point.x - size * Constants.tileW;
			tile.y = point.y - (size * Constants.tileH);
			
			var colorTransform: ColorTransform;
			if (tile.x == 0 && tile.y == 0)
				colorTransform = new ColorTransform(1.0, 1.0, 1.0, 1.0, 150, 150, 30);
			else
				colorTransform = new ColorTransform(1.0, 1.0, 1.0, 1.0, 0, 100);
			
			tile.bitmapData.draw(tiledata, null, colorTransform);			
			
			circle.addChild(tile);
			
			tiles.push(tile);		
		}

	}
	
}
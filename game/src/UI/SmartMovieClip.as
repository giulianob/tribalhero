package src.UI {
	import flash.display.DisplayObject;
	import flash.display.MovieClip;
	import flash.events.Event;

	/**
	* ...
	* @author Default
	*/
	public class SmartMovieClip extends MovieClip
	{		
		private static const DEBUG: Boolean = false;
		
		public var tag: Object;
		
		public function SmartMovieClip() {
			if (DEBUG)
				drawDebug();								
		}

		protected function drawDebug(): void
		{					
			this.graphics.lineStyle(1, 0xFF0000);
			
			for (var i: int = 0; i < numChildren; i++)
			{
				var item: DisplayObject = getChildAt(i);
				this.graphics.drawRect(item.x, item.y, item.width, item.height);				
			}
		}
		
		public function getSmartWidth(): Number
		{
			var biggestX: DisplayObject;;
			var originX: DisplayObject;
			
			for (var i: int = 0; i < numChildren; i++)
			{
				var item: DisplayObject = getChildAt(i);

				if (originX == null || item.x < originX.x)
					originX = item;				
					
				if (biggestX == null || item.width + item.x > biggestX.width + biggestX.x)
					biggestX = item;					
			}
			
			if (biggestX == null || originX == null)
				return 0;
			else
				return (biggestX.width + biggestX.x) - originX.x;
		}
		
		public function getSmartHeight(): Number
		{
			var biggestY: DisplayObject;
			var originY: DisplayObject;
			
			for (var i: int = 0; i < numChildren; i++)
			{
				var item: DisplayObject = getChildAt(i);
				
				if (originY == null || item.y < originY.y)
					originY = item;
					
				if (biggestY == null || item.height + item.y > biggestY.height + biggestY.y)
					biggestY = item;					
			}
			
			if (biggestY == null || originY == null)
				return 0;
			else
				return (biggestY.height + biggestY.y) - originY.y;
		}
		
	}
	
}
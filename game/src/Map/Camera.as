package src.Map
{
	import flash.events.Event;
	import flash.events.EventDispatcher;
	import src.Constants;
	
	public class Camera extends EventDispatcher
	{
		public static const ON_MOVE: String = "ON_MOVE";
		
		public var x:int;
		public var y:int;		
		
		//Allows us to cue a position and return to it later
		private var cueX: int;
		private var cueY: int;
		
		public function reset(): void
		{
			x = 0;
			y = 0;
		}
		
		public function cue(): void {
			cueX = x;
			cueY = y;
		}
		
		public function goToCue(): void {
			x = cueX;
			y = cueY;
		}
		
		public function get miniMapX(): int
		{			
			return (x / Constants.tileW) * Constants.miniMapTileW;
		}
		
		public function get miniMapY(): int
		{
			return (y / Constants.tileH) * Constants.miniMapTileH;
		}
		
		public function Camera(x: int, y: int)
		{
			this.x = x;
			this.y = y;
		}
		
		public function MoveLeft(step :int): void
		{
			MoveLeftEx(step, true);
		}
		
		private function MoveLeftEx(step: int, callback: Boolean): void
		{
			x -= step;
			
			if (x < 0)
				x = 0;
				
			if (callback)
				dispatchEvent(new Event(ON_MOVE));
		}
		
		public function MoveRight(step: int): void
		{
			MoveRightEx(step, true);
		}
		
		private  function MoveRightEx(step: int, callback: Boolean): void
		{
			x += step;
			
			if (x > Constants.mapW - Constants.screenW - (Constants.tileW / 2))
				x = Constants.mapW - Constants.screenW - (Constants.tileW / 2);
		
			if (callback)
				dispatchEvent(new Event(ON_MOVE));
		}
		
		public function MoveDown(step: int): void
		{
			MoveDownEx(step, true);
		}
		
		private function MoveDownEx(step: int, callback: Boolean): void
		{
			y += step;
			
			if (y > Constants.mapTileH * int(Constants.tileH / 2) - Constants.screenH - int(Constants.tileH / 2) )
				y = Constants.mapTileH * int(Constants.tileH / 2) - Constants.screenH - int(Constants.tileH / 2);
				
			if (callback)
				dispatchEvent(new Event(ON_MOVE));
		}
		
		public function MoveUp(step: int): void
		{
			MoveUpEx(step, true);
		}
		
		private function MoveUpEx(step: int, callback: Boolean): void
		{
			y -= step;
				
			if (y < 0)
				y = 0;
				
			if (callback)
				dispatchEvent(new Event(ON_MOVE));
		}
		
		public function Move(dx: int, dy: int): void
		{
			if (dx > 0)
				MoveRightEx(dx, false);
			else
				MoveLeftEx(-dx, false);
				
			if (dy > 0)
				MoveDownEx(dy, false);
			else
				MoveUpEx(-dy, false);
				
			dispatchEvent(new Event(ON_MOVE));
		}
		
		public function ScrollToCenter(x: int, y: int): void 
		{			
			ScrollTo(x - Constants.screenW / 2, y - Constants.screenH / 2);
		}
		
		public function ScrollTo(x: int, y: int): void
		{
			if (x < 0) x = 0;
			if (y < 0) y = 0;
			
			if (x > Constants.mapW - Constants.screenW - (Constants.tileW / 2))
				x = Constants.mapW - Constants.screenW - (Constants.tileW / 2);
		
			if (y > Constants.mapTileH * int(Constants.tileH / 2) - Constants.screenH - int(Constants.tileH / 2) )
				y = Constants.mapTileH * int(Constants.tileH / 2) - Constants.screenH - int(Constants.tileH / 2);
							
			this.x = x;
			this.y = y;
			
			dispatchEvent(new Event(ON_MOVE));
		}
	}
}
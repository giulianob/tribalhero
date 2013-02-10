package src.Map
{
	import flash.events.Event;
	import flash.events.EventDispatcher;
	import flash.geom.Point;
	import src.Constants;

	public class Camera extends EventDispatcher
	{
		public static const ON_MOVE: String = "ON_MOVE";

		public var x:int;
		public var y:int;

		// Saves the x,y to see if we need to
		private var isDirty: Boolean = false;
		private var updating: Boolean = false;

		//Allows us to cue a position and return to it later
		private var cueX: int;
		private var cueY: int;
		
		//Zooming factor
		private var zoomFactor: Number = 1;
		private var zoomFactorOverOne: Number = (1.0 / zoomFactor);

		public function beginMove(): void {
			if (updating) return;

			updating = true;
		}

		public function endMove(): void {
			updating = false;

			if (!isDirty) return;

			isDirty = false;
			dispatchEvent(new Event(ON_MOVE));
		}

		private function fireOnMove() : void {
			if (updating) {
				isDirty = true;
				return;
			}

			dispatchEvent(new Event(ON_MOVE));
		}

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
		
		public function get miniMapCenter(): Point
		{			
			var point: Point = GetCenter();
			return new Point((point.x / Constants.tileW) * Constants.miniMapTileW, (point.y / Constants.tileH) * Constants.miniMapTileH);
		}

		public function Camera(x: int, y: int)
		{
			this.x = x;
			this.y = y;
		}

		public function MoveLeft(step :int): void
		{
			MoveLeftEx(step);
		}

		private function MoveLeftEx(step: int): void
		{
			x -= step;

			if (x < 0)
			x = 0;
			
			fireOnMove();
		}

		public function MoveRight(step: int): void
		{
			MoveRightEx(step);
		}

		private  function MoveRightEx(step: int): void
		{
			x += step;

			if (x > Constants.mapW - Constants.screenW - (Constants.tileW / 2))
			x = Constants.mapW - Constants.screenW - (Constants.tileW / 2);
			
			fireOnMove();
		}

		public function MoveDown(step: int): void
		{
			MoveDownEx(step);
		}

		private function MoveDownEx(step: int): void
		{
			y += step;

			if (y > Constants.mapTileH * int(Constants.tileH / 2) - Constants.screenH - int(Constants.tileH / 2) )
			y = Constants.mapTileH * int(Constants.tileH / 2) - Constants.screenH - int(Constants.tileH / 2);
			
			fireOnMove();
		}

		public function MoveUp(step: int): void
		{
			MoveUpEx(step);			
		}

		private function MoveUpEx(step: int): void
		{
			y -= step;

			if (y < 0) y = 0;
			
			fireOnMove();
		}

		public function Move(dx: int, dy: int): void
		{
			beginMove();

			if (dx > 0)
			MoveRightEx(dx);
			else
			MoveLeftEx(-dx);

			if (dy > 0)
			MoveDownEx(dy);
			else
			MoveUpEx(-dy);

			endMove();
		}

		public function ScrollToCenter(x: int, y: int): void
		{
			ScrollTo(x - (Constants.screenW * zoomFactorOverOne) / 2.0, y - (Constants.screenH * zoomFactorOverOne) / 2.0);
		}
		
		public function GetCenter(): Point
		{
			return new Point(x + (Constants.screenW * zoomFactorOverOne) / 2.0, y + (Constants.screenH * zoomFactorOverOne) / 2.0);
		}

		public function ScrollTo(x: int, y: int): void
		{
			if (x < 0) x = 0;
			if (y < 0) y = 0;

			this.x = Math.min(x, Constants.mapW - (Constants.screenW * zoomFactorOverOne) - (Constants.tileW / 2));

			this.y = Math.min(y, Constants.mapTileH * int(Constants.tileH / 2) - (Constants.screenH * zoomFactorOverOne) - int(Constants.tileH / 2));

			fireOnMove();
		}
		
		public function setZoomFactor(factor: Number): void {
			zoomFactor = factor;
			zoomFactorOverOne = (1.0 / factor);
			ScrollTo(x, y);
		}
		
		public function getZoomFactorOverOne(): Number {
			return zoomFactorOverOne;
		}
		
		public function getZoomFactor(): Number {
			return zoomFactor;
		}
		
	}
}


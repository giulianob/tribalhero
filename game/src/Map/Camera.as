package src.Map
{
    import flash.events.Event;
    import flash.events.EventDispatcher;
    import flash.geom.Point;
    import flash.geom.Rectangle;

    import src.Constants;

    public class Camera extends EventDispatcher
	{
		public static const ON_MOVE: String = "ON_MOVE";

		public var currentPosition: ScreenPosition = new ScreenPosition();

		// Saves the x,y to see if we need to
		private var isDirty: Boolean = false;
		private var updating: Boolean = false;

		//Allows us to cue a position and return to it later
		private var cuePosition: ScreenPosition;
		
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
			currentPosition = new ScreenPosition();
		}

		public function cue(): void {
			cuePosition = currentPosition.copy();
		}

		public function goToCue(): void {
			currentPosition = cuePosition.copy();
		}

		public function get miniMapX(): int
		{
			return (currentPosition.x / Constants.tileW) * Constants.miniMapTileW;
		}

		public function get miniMapY(): int
		{
			return (currentPosition.y / Constants.tileH) * Constants.miniMapTileH;
		}
		
		public function get miniMapCenter(): Point
		{			
			var point: ScreenPosition = GetCenter();
			return new Point((point.x / Constants.tileW) * Constants.miniMapTileW, (point.y / Constants.tileH) * Constants.miniMapTileH);
		}

		public function Camera(x: int, y: int)
		{
			currentPosition.x = x;
            currentPosition.y = y;
		}

		public function MoveLeft(step :int): void
		{
			MoveLeftEx(step);
		}

		private function MoveLeftEx(step: int): void
		{
            currentPosition.x -= step;

			if (currentPosition.x < 0) {
                currentPosition.x = 0;
            }

			fireOnMove();
		}

		public function MoveRight(step: int): void
		{
			MoveRightEx(step);
		}

		private  function MoveRightEx(step: int): void
		{
            currentPosition.x += step;

			if (currentPosition.x > Constants.mapW - Constants.screenW - (Constants.tileW / 2)) {
                currentPosition.x = Constants.mapW - Constants.screenW - (Constants.tileW / 2);
            }
			
			fireOnMove();
		}

		public function MoveDown(step: int): void
		{
			MoveDownEx(step);
		}

		private function MoveDownEx(step: int): void
		{
            currentPosition.y += step;

			if (currentPosition.y > Constants.mapTileH * int(Constants.tileH / 2) - Constants.screenH - int(Constants.tileH / 2)) {
                currentPosition.y = Constants.mapTileH * int(Constants.tileH / 2) - Constants.screenH - int(Constants.tileH / 2);
            }
			
			fireOnMove();
		}

		public function MoveUp(step: int): void
		{
			MoveUpEx(step);			
		}

		private function MoveUpEx(step: int): void
		{
            currentPosition.y -= step;

			if (currentPosition.y < 0)  {
                currentPosition.y = 0;
            }
			
			fireOnMove();
		}

		public function Move(dx: int, dy: int): void
		{
			beginMove();

			if (dx > 0) {
			    MoveRightEx(dx);
            }
			else {
			    MoveLeftEx(-dx);
            }

			if (dy > 0) {
			    MoveDownEx(dy);
            }
			else {
			    MoveUpEx(-dy);
            }

			endMove();
		}

		public function ScrollToCenter(screenPos: ScreenPosition): void
		{
			ScrollTo(new ScreenPosition(screenPos.x - (Constants.screenW * zoomFactorOverOne) / 2.0, screenPos.y - (Constants.screenH * zoomFactorOverOne) / 2.0));
		}
		
		public function GetCenter(): ScreenPosition
		{
			return new ScreenPosition(currentPosition.x + (Constants.screenW * zoomFactorOverOne) / 2.0, currentPosition.y + (Constants.screenH * zoomFactorOverOne) / 2.0);
		}

		public function CameraRectangle() : Rectangle
		{
			return new Rectangle(currentPosition.x, currentPosition.y, Constants.screenW * zoomFactorOverOne, Constants.screenH * zoomFactorOverOne);
		}
				
		public function ScrollTo(screenPos: ScreenPosition): void
		{
			if (currentPosition.x < 0) currentPosition.x = 0;
			if (currentPosition.y < 0) currentPosition.y = 0;

			currentPosition.x = Math.min(screenPos.x, Constants.mapW - (Constants.screenW * zoomFactorOverOne) - (Constants.tileW / 2));

			currentPosition.y = Math.min(screenPos.y, Constants.mapTileH * int(Constants.tileH / 2) - (Constants.screenH * zoomFactorOverOne) - int(Constants.tileH / 2));

			fireOnMove();
		}
		
		public function setZoomFactor(factor: Number): void {
			zoomFactor = factor;
			zoomFactorOverOne = (1.0 / factor);
			ScrollTo(currentPosition);
		}
		
		public function getZoomFactorOverOne(): Number {
			return zoomFactorOverOne;
		}
		
		public function getZoomFactor(): Number {
			return zoomFactor;
		}
		
	}
}


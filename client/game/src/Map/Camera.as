package src.Map
{
    import feathers.utils.math.clamp;

    import flash.events.Event;
    import flash.events.EventDispatcher;
    import flash.geom.Point;
    import flash.geom.Rectangle;

    import mx.utils.StringUtil;

    import src.Constants;
    import src.Util.Util;

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
		private var _zoomFactor: int = 75;
		private var zoomFactorOverOne: Number = (1.0 / (_zoomFactor / 100.0));

        public var scrollRate: Number = 1;

        public function Camera(x: Number, y: Number)
        {
            currentPosition.setXAsNumber(x);
            currentPosition.setYAsNumber(y);
        }

		public function beginMove(): void {
			if (updating) return;

			updating = true;
		}

		public function endMove(): void {
			updating = false;

			if (!isDirty) {
                return;
            }

			isDirty = false;

			fireOnMove();
		}

		private function fireOnMove() : void {
			if (updating) {
				isDirty = true;
				return;
			}

            //if (Constants.debug >= 2) {
                Util.log(StringUtil.substitute("Camera moved to {0},{1} zoomFactor:{2}", currentPosition.x, currentPosition.y, zoomFactor));
            //}

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
			var point: ScreenPosition = mapCenter();
			return new Point((point.x / Constants.tileW) * Constants.miniMapTileW, (point.y / Constants.tileH) * Constants.miniMapTileH);
		}

		public function moveLeft(step :int): void
		{
			moveLeftEx(step);
		}

		private function moveLeftEx(step: int): void
		{
            currentPosition.x -= step;

			if (currentPosition.x < 0) {
                currentPosition.x = 0;
            }

			fireOnMove();
		}

		public function moveRight(step: int): void
		{
			moveRightEx(step);
		}

		private function moveRightEx(step: int): void
		{
            currentPosition.x += step;

			if (currentPosition.x > Constants.mapW - Constants.screenW - (Constants.tileW / 2)) {
                currentPosition.x = Constants.mapW - Constants.screenW - (Constants.tileW / 2);
            }
			
			fireOnMove();
		}

		public function moveDown(step: int): void
		{
			moveDownEx(step);
		}

		private function moveDownEx(step: int): void
		{
            currentPosition.y += step;

			if (currentPosition.y > Constants.mapTileH * int(Constants.tileH / 2) - Constants.screenH - int(Constants.tileH / 2)) {
                currentPosition.y = Constants.mapTileH * int(Constants.tileH / 2) - Constants.screenH - int(Constants.tileH / 2);
            }
			
			fireOnMove();
		}

		public function moveUp(step: int): void
		{
			moveUpEx(step);
		}

		private function moveUpEx(step: int): void
		{
            currentPosition.y -= step;

			if (currentPosition.y < 0)  {
                currentPosition.y = 0;
            }
			
			fireOnMove();
		}

		public function move(dx: int, dy: int): void
		{
			beginMove();

			if (dx > 0) {
			    moveRightEx(dx);
            }
			else {
			    moveLeftEx(-dx);
            }

			if (dy > 0) {
			    moveDownEx(dy);
            }
			else {
			    moveUpEx(-dy);
            }

			endMove();
		}

		public function scrollToCenter(screenPos: ScreenPosition): void
		{
            var zoomFactorPercentage: Number = getZoomFactorPercentage();
			scrollTo(new ScreenPosition(
                    screenPos.getXAsNumber() - (Number(Constants.screenW) / zoomFactorPercentage) / 2.0,
                    screenPos.getYAsNumber() - (Constants.screenH / zoomFactorPercentage) / 2.0));
		}
		
		public function mapCenter(): ScreenPosition
		{
            var zoomFactorPercentage: Number = getZoomFactorPercentage();
            return new ScreenPosition(
                    currentPosition.getXAsNumber() + (Number(Constants.screenW) / zoomFactorPercentage) / 2.0,
                    currentPosition.getYAsNumber() + (Constants.screenH / zoomFactorPercentage) / 2.0);
		}

		public function cameraRectangle() : Rectangle
		{
			return new Rectangle(
                    currentPosition.x,
                    currentPosition.y,
                    Constants.screenW * zoomFactorOverOne,
                    Constants.screenH * zoomFactorOverOne);
		}

		public function scrollTo(screenPos: ScreenPosition): void
		{
			if (currentPosition.x < 0) currentPosition.x = 0;
			if (currentPosition.y < 0) currentPosition.y = 0;

			currentPosition.setXAsNumber(Math.min(screenPos.getXAsNumber(), int(Constants.mapW - (Constants.screenW * zoomFactorOverOne) - (Constants.tileW / 2.0))));
			currentPosition.setYAsNumber(Math.min(screenPos.getYAsNumber(), int(Constants.mapTileH * int(Constants.tileH / 2.0) - (Constants.screenH * zoomFactorOverOne) - int(Constants.tileH / 2.0))));

			fireOnMove();
		}

        //noinspection JSUnusedGlobalSymbols
        public function set zoomFactor(factor: int): void {
            _zoomFactor = clamp(factor, 50, 100);
            zoomFactorOverOne = (1.0 / getZoomFactorPercentage());
            scrollTo(currentPosition);
        }

        public function get zoomFactor(): int {
            return _zoomFactor;
        }

		public function getZoomFactorOverOne(): Number {
			return zoomFactorOverOne;
		}
		
		public function getZoomFactorPercentage(): Number {
			return zoomFactor / 100.0;
		}
		
	}
}


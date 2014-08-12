package src.UI.Components {

    import src.Map.Position;
    import src.Map.ScreenPosition;
    import src.Map.TileLocator;

    public class GroundCircle extends GroundCallbackCircle
	{
        public static const GREEN: uint = 0x7D9F43;
        public static const SELECTED_GREEN: uint = 0x1A4C0D;
        public static const UNBUILDABLE: uint = 0xdda100;
        public static const RANGE: uint = 0xff6000;

        private var color: uint;
        private var tileLocator: TileLocator;
        private var radius: int;

		public function GroundCircle(radius: int, mainPosition: ScreenPosition = null, color: uint = SELECTED_GREEN, skipCenter: Boolean = false) {
			super(radius, mainPosition ? mainPosition : new ScreenPosition(), callback, skipCenter);

            this.radius = radius;
			this.color = color;
            this.tileLocator = new TileLocator();
		}

        private function callback(x: int, y: int): * {
            var position: Position = mainPosition.toPosition();
            var tilePosition: Position = new ScreenPosition(x, y).toPosition();

            var distance: int = TileLocator.radiusDistance(position.x, position.y, 1, tilePosition.x, tilePosition.y, 1);

            if (distance > radius) {
                return false;
            }

            return color;
        }
	}

}


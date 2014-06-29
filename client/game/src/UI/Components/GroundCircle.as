package src.UI.Components {
    import flash.geom.ColorTransform;

    import src.Objects.Factories.SpriteFactory;

    import starling.display.*;

    import src.Constants;
    import src.Map.Position;
    import src.Map.ScreenPosition;
    import src.Map.TileLocator;
    import src.Objects.SimpleObject;

    import starling.events.Event;

    public class GroundCircle extends SimpleObject
	{
		private var circle: DisplayObjectContainer;
		private var skipCenter: Boolean;
        private var radius: int;
        private var color: uint;

		public function GroundCircle(radius: int, skipCenter: Boolean = false, color: uint = 0x00FF00) {
			super( -10, -10, 1);

			this.color = color;
			this.skipCenter = skipCenter;
            this.radius = radius;

			drawCircle(radius);
			addEventListener(Event.REMOVED_FROM_STAGE, onRemovedFromStage);
			addEventListener(Event.ADDED_TO_STAGE, onAddedToStage);
		}

		private function onAddedToStage(e: Event):void
		{
			drawCircle(radius);
		}

		public function onRemovedFromStage(e: Event):void
		{
			dispose();
		}

		private function drawCircle(radius: int):void
		{
			this.radius = radius;

			if (circle != null)
			dispose();

			circle = new Sprite();

			for each (var position: Position in TileLocator.foreachRadius(Math.ceil(radius / 2.0), Math.ceil(radius / 2.0) * 2 + 1, radius)) {
                var tile: Image = SpriteFactory.getStarlingImage("MASK_TILE");
                tile.color = color;

                var tileRadius: int = Math.ceil(radius / 2.0);
                var point: ScreenPosition = position.toScreenPosition();
                tile.x = point.x - tileRadius * Constants.tileW;
                tile.y = point.y - tileRadius * Constants.tileH;

                if (tile.x == 0 && tile.y == 0 && skipCenter) {
                    continue;
                }

                circle.addChild(tile);
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
		}
	}

}


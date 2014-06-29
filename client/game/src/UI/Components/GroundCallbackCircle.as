package src.UI.Components {
    import starling.display.*;

    import src.Constants;
    import src.Map.Position;
    import src.Map.ScreenPosition;
    import src.Map.TileLocator;
    import src.Objects.Factories.SpriteFactory;
    import src.Objects.SimpleObject;

    import starling.display.Image;

    import starling.events.Event;

    public class GroundCallbackCircle extends SimpleObject
	{
		private var circle: DisplayObjectContainer;
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

			circle = new Sprite();
			for each (var position: Position in TileLocator.foreachTile(size, size * 2 + 1, size)) {
                var tile: Image = SpriteFactory.getStarlingImage("MASK_TILE");
                var point: ScreenPosition = position.toScreenPosition();
                tile.x = point.x - size * Constants.tileW;
                tile.y = point.y - (size * Constants.tileH);
                var colorTransform: * = callback(tile.x, tile.y);

                if (colorTransform == false) {
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


package src.UI.Cursors {
    import flash.geom.Point;

    import src.Global;
    import src.Map.MapOverlayBase;
    import src.Map.ScreenPosition;
    import src.Map.TileLocator;
    import src.Objects.Factories.ObjectFactory;
    import src.Objects.Forest;
    import src.Objects.ObjectContainer;
    import src.Objects.SimpleGameObject;
    import src.UI.Components.GroundCircle;
    import src.UI.Sidebars.CursorCancel.CursorCancelSidebar;

    import starling.events.Event;
    import starling.events.Touch;
    import starling.events.TouchEvent;
    import starling.events.TouchPhase;

    public class GroundForestCursor extends MapOverlayBase
	{
		private var objPosition: ScreenPosition = new ScreenPosition();

		private var cursor: GroundCircle;

		private var highlightedObj: Forest;

		private var onAccept: Function;

        public function GroundForestCursor(onAccept: Function): void {
            this.onAccept = onAccept;

            Global.map.selectObject(null);
            Global.map.objContainer.resetObjects();

            var size: int = 0;

            cursor = new GroundCircle(size);

            var sidebar: CursorCancelSidebar = new CursorCancelSidebar(null);
            Global.gameContainer.setSidebar(sidebar);

            addEventListener(Event.ADDED_TO_STAGE, onAddedToStage);
            addEventListener(TouchEvent.TOUCH, onTouched);

            Global.gameContainer.setOverlaySprite(this);
        }

		public function onAddedToStage(e: Event):void
		{
			moveTo(Global.stage.mouseX, Global.stage.mouseY);
		}

		override public function dispose():void
		{
            super.dispose();

			if (cursor != null)
			{
				cursor.dispose();
			}

			Global.gameContainer.message.hide();

			if (highlightedObj)
			{
				highlightedObj.setHighlighted(false);
				highlightedObj = null;
			}

            removeEventListener(Event.ADDED_TO_STAGE, onAddedToStage);
            removeEventListener(TouchEvent.TOUCH, onTouched);
		}

        public function onTouched(event: TouchEvent): void {
            var clickTouch: Touch = event.getTouch(this, TouchPhase.BEGAN);
            if (clickTouch && clickTouch.tapCount == 2) {
                onDoubleClicked();
            }

            var moveTouch: Touch = event.getTouch(this, TouchPhase.HOVER);
            if (moveTouch) {
                var mousePos: Point = TileLocator.getPointWithZoomFactor(moveTouch.globalX, moveTouch.globalY);
                moveTo(mousePos.x, mousePos.y);
            }

            event.stopImmediatePropagation();
        }

		public function onDoubleClicked():void
		{
			var objects: Array = Global.map.regions.getObjectsInTile(objPosition.toPosition());

			if (objects.length == 0) return;

			var gameObj: SimpleGameObject = objects[0];

			if (ObjectFactory.getClassType(gameObj.type) != ObjectFactory.TYPE_FOREST) return;

			Global.gameContainer.setOverlaySprite(null);
			Global.gameContainer.setSidebar(null);
			Global.map.selectObject(null);

			onAccept(gameObj);
		}

		public function moveTo(x: int, y: int):void
		{
			var pos: ScreenPosition = TileLocator.getActualCoord(
                    Global.gameContainer.camera.currentPosition.x + Math.max(x, 0),
                    Global.gameContainer.camera.currentPosition.y + Math.max(y, 0));

			if (!pos.equals(objPosition))
			{
				objPosition = pos;

                cursor.moveTo(pos);

				validate();
			}
		}

		public function validate():void
		{
			if (highlightedObj)
			{
				highlightedObj.setHighlighted(false);
				highlightedObj = null;
			}

			var objects: Array = Global.map.regions.getObjectsInTile(objPosition.toPosition(), Forest);

			if (objects.length == 0) {
				Global.gameContainer.message.showMessage("Choose a forest to gather wood from.");
				return;
			}

			var forestObj: Forest = objects[0];
			forestObj.setHighlighted(true);
			highlightedObj = forestObj;

            Global.gameContainer.message.showMessage("Double click this forest to gather wood.");
		}
	}

}


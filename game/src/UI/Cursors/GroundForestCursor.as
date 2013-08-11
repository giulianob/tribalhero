package src.UI.Cursors {
    import flash.display.MovieClip;
    import flash.events.Event;
    import flash.events.MouseEvent;
    import flash.geom.Point;

    import src.Global;
    import src.Map.ScreenPosition;
    import src.Map.TileLocator;
    import src.Objects.Factories.ObjectFactory;
    import src.Objects.Forest;
    import src.Objects.IDisposable;
    import src.Objects.ObjectContainer;
    import src.Objects.SimpleGameObject;
    import src.UI.Components.GroundCircle;
    import src.UI.Sidebars.CursorCancel.CursorCancelSidebar;

    public class GroundForestCursor extends MovieClip implements IDisposable
	{
		private var objPosition: ScreenPosition = new ScreenPosition();

		private var originPoint: Point;

		private var cursor: GroundCircle;

		private var highlightedObj: Forest;

		private var onAccept: Function;

        public function GroundForestCursor(onAccept: Function): void {
            this.onAccept = onAccept;
            doubleClickEnabled = true;

            Global.map.selectObject(null);
            Global.map.objContainer.resetObjects();

            var size: int = 0;

            cursor = new GroundCircle(size);
            cursor.alpha = 0.6;

            Global.map.objContainer.addObject(cursor, ObjectContainer.LOWER);

            var sidebar: CursorCancelSidebar = new CursorCancelSidebar(null);
            Global.gameContainer.setSidebar(sidebar);

            addEventListener(Event.ADDED_TO_STAGE, onAddedToStage);
            addEventListener(MouseEvent.DOUBLE_CLICK, onMouseDoubleClick);
            addEventListener(MouseEvent.CLICK, onMouseStop, true);
            addEventListener(MouseEvent.MOUSE_MOVE, onMouseMove);
            addEventListener(MouseEvent.MOUSE_OVER, onMouseStop);
            addEventListener(MouseEvent.MOUSE_DOWN, onMouseDown);

            Global.gameContainer.setOverlaySprite(this);
        }

		public function onAddedToStage(e: Event):void
		{
			moveTo(stage.mouseX, stage.mouseY);
		}

		public function dispose():void
		{
			if (cursor != null)
			{
				Global.map.objContainer.removeObject(cursor, ObjectContainer.LOWER);
				cursor.dispose();
			}

			Global.gameContainer.message.hide();

			if (highlightedObj)
			{
				highlightedObj.setHighlighted(false);
				highlightedObj = null;
			}
		}

		public function onMouseStop(event: MouseEvent):void
		{
			event.stopImmediatePropagation();
		}

		public function onMouseDoubleClick(event: MouseEvent):void
		{
			if (Point.distance(TileLocator.getPointWithZoomFactor(event.stageX, event.stageY), originPoint) > 4) return;

			event.stopImmediatePropagation();

			var objects: Array = Global.map.regions.getObjectsInTile(objPosition.toPosition());

			if (objects.length == 0) return;

			var gameObj: SimpleGameObject = objects[0];

			if (ObjectFactory.getClassType(gameObj.type) != ObjectFactory.TYPE_FOREST) return;

			Global.gameContainer.setOverlaySprite(null);
			Global.gameContainer.setSidebar(null);
			Global.map.selectObject(null);

			onAccept(gameObj);
		}

		public function onMouseDown(event: MouseEvent):void
		{
			originPoint = TileLocator.getPointWithZoomFactor(event.stageX, event.stageY);
		}

		public function onMouseMove(event: MouseEvent):void
		{
			if (event.buttonDown) {
				return;
			}

			var mousePos: Point = TileLocator.getPointWithZoomFactor(event.stageX, event.stageY);
			moveTo(mousePos.x, mousePos.y);
		}

		public function moveTo(x: int, y: int):void
		{
			var pos: ScreenPosition = TileLocator.getActualCoord(
                    Global.gameContainer.camera.currentPosition.x + Math.max(x, 0),
                    Global.gameContainer.camera.currentPosition.y + Math.max(y, 0));

			if (!pos.equals(objPosition))
			{
				Global.map.objContainer.removeObject(cursor, ObjectContainer.LOWER);

				objPosition = pos;

				cursor.objX = pos.x;
				cursor.objY = pos.y;

				Global.map.objContainer.addObject(cursor, ObjectContainer.LOWER);

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


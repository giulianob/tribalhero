package src.UI.Cursors {
    import src.FeathersUI.Map.MapOverlayBase;

    import starling.events.*;
    import flash.geom.Point;

    import org.aswing.JOptionPane;

    import src.Global;
    import src.Map.City;
    import src.Map.ScreenPosition;
    import src.Map.TileLocator;
    import src.Objects.ObjectContainer;
    import src.Objects.StructureObject;
    import src.UI.Components.GroundCircle;
    import src.UI.Dialog.InfoDialog;
    import src.UI.Sidebars.CursorCancel.CursorCancelSidebar;

    public class StructureDowngradeCursor extends MapOverlayBase
	{
		private var objPosition: ScreenPosition = new ScreenPosition();

		private var cursor: GroundCircle;

		private var city: City;

		private var highlightedObj: StructureObject;

		private var parentObj: StructureObject;

		public function StructureDowngradeCursor(parentObj: StructureObject)
		{
			this.parentObj = parentObj;

			city = Global.map.cities.get(parentObj.cityId);

			Global.map.selectObject(null);
			Global.map.objContainer.resetObjects();

			cursor = new GroundCircle(0);

			var sidebar: CursorCancelSidebar = new CursorCancelSidebar(parentObj);
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
			var objects: Array = Global.map.regions.getObjectsInTile(objPosition.toPosition(), StructureObject);

			if (objects.length == 0) return;

			var gameObj: StructureObject = objects[0];

			if (gameObj.cityId != parentObj.cityId) return;

			InfoDialog.showMessageDialog("Confirm", "Are you sure? Your structure is about to be completely removed.", function(result: int): void {				
				if (result == JOptionPane.YES)						
					Global.mapComm.Objects.downgrade(city.id, parentObj.objectId, gameObj.objectId);
					
			}, null, true, true, JOptionPane.YES | JOptionPane.NO);

			Global.gameContainer.setOverlaySprite(null);
			Global.gameContainer.setSidebar(null);
			Global.map.selectObject(null);
		}

		public function moveTo(x: int, y: int):void
		{
			var pos: ScreenPosition = TileLocator.getActualCoord(Global.gameContainer.camera.currentPosition.x + Math.max(x, 0), Global.gameContainer.camera.currentPosition.y + Math.max(y, 0));

            // Don't do anything if position hasn't changed since last time the mouse moved
            if (pos.equals(objPosition)) {
                return;
            }

            objPosition = pos;

            cursor.moveTo(pos);

            if (highlightedObj) {
                highlightedObj.setHighlighted(false);
                highlightedObj = null;
            }

            var objects: Array = Global.map.regions.getObjectsInTile(objPosition.toPosition(), StructureObject);
            if (objects.length == 0 || objects[0].cityId != parentObj.cityId) {
                Global.gameContainer.message.showMessage("Choose a structure to remove");
                return;
            }

            var gameObj: StructureObject = objects[0];
            gameObj.setHighlighted(true);
            highlightedObj = gameObj;

            Global.gameContainer.message.showMessage("Double click to remove this structure.");
		}

    }

}


package src.UI.Cursors {
    import src.FeathersUI.Map.MapOverlayBase;

    import starling.events.*;
    import flash.geom.*;

    import src.*;
    import src.Map.*;
    import src.Objects.*;
    import src.Objects.Effects.*;
    import src.Objects.Factories.*;
    import src.Objects.Stronghold.Stronghold;
    import src.Objects.Troop.*;
    import src.UI.Components.*;
    import src.UI.Tooltips.*;
    import src.Util.*;

    public class GroundReinforceCursor extends MapOverlayBase
	{
		private var objPosition: ScreenPosition = new ScreenPosition();

		private var cursor: GroundCircle;

		private var city: City;

		private var troopSpeed: Number;

		private var highlightedObj: SimpleGameObject;

		private var tooltip: Tooltip;
		
		private var onAccept: Function;

		public function GroundReinforceCursor(city: City, onAccept: Function, troop: TroopStub):void {
            this.city = city;
            this.onAccept = onAccept;

            Global.map.selectObject(null);
            Global.map.objContainer.resetObjects();

            troopSpeed = troop.getSpeed(city);

            var size: int = 0;

            cursor = new GroundCircle(size);

            addEventListener(Event.ADDED_TO_STAGE, onAddedToStage);
            addEventListener(TouchEvent.TOUCH, onTouched);

            Global.gameContainer.setOverlaySprite(this);
        }
		
		public function getTargetObject(): SimpleGameObject
		{
			return highlightedObj;
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

			if (tooltip) tooltip.hide();

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
			if (highlightedObj == null)
				return;

			if (onAccept != null)
				onAccept(this);
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
			
			if (tooltip) {
				tooltip.hide();
				tooltip = null;			
			}						

			var objects: Array = Global.map.regions.getObjectsInTile(objPosition.toPosition(), [StructureObject, Stronghold]);
		
			if (objects.length == 0) {
				Global.gameContainer.message.showMessage(StringHelper.localize("REINFORCE_CHOOSE_TARGET"));
				return;
			}				

			var gameObj: SimpleGameObject = objects[0];

			if (gameObj is StructureObject) {
				var structObj: StructureObject = gameObj as StructureObject;		
				if (structObj.cityId == city.id || !structObj.isMainBuilding) {
					Global.gameContainer.message.showMessage(StringHelper.localize("REINFORCE_CHOOSE_TARGET"));
					return;					
				}
				
				tooltip = new StructureTooltip(structObj, StructureFactory.getPrototype(structObj.type, structObj.level));
				tooltip.show(structObj);				
			}
			else if (gameObj is Stronghold) {
				tooltip = new StrongholdTooltip(gameObj as Stronghold);
                tooltip.show(gameObj);
			}

            cursor.clear();

			gameObj.setHighlighted(true);
			highlightedObj = gameObj;

			var targetMapDistance: Position = gameObj.primaryPosition.toPosition();
			var distance: int = TileLocator.distance(city.primaryPosition.x, city.primaryPosition.y, 1, targetMapDistance.x, targetMapDistance.y, 1);
			var timeAwayInSeconds: int = Formula.moveTimeTotal(city, troopSpeed, distance, false);			

            var message: String = StringHelper.localize("REINFORCE_DISTANCE_MESSAGE", DateUtil.niceTime(timeAwayInSeconds));

			if (Constants.debug) {
				message += "Speed [" +troopSpeed+"] Distance [" + distance + "] in " + timeAwayInSeconds + " sec("+DateUtil.formatTime(timeAwayInSeconds)+")";
            }

            Global.gameContainer.message.showMessage(message);
		}
	}

}


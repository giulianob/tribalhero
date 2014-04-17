﻿package src.UI.Cursors {
    import flash.display.*;
    import flash.events.*;
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

    public class GroundReinforceCursor extends MovieClip implements IDisposable
	{
		private var objPosition: ScreenPosition = new ScreenPosition();

		private var originPoint: Point;

		private var cursor: GroundCircle;

		private var city: City;

		private var troopSpeed: Number;

		private var highlightedObj: SimpleGameObject;

		private var tooltip: Tooltip;
		
		private var onAccept: Function;

		public function GroundReinforceCursor(city: City, onAccept: Function, troop: TroopStub):void {
            doubleClickEnabled = true;

            this.city = city;
            this.onAccept = onAccept;

            Global.map.selectObject(null);
            Global.map.objContainer.resetObjects();

            troopSpeed = troop.getSpeed(city);

            var size: int = 0;

            cursor = new GroundCircle(size);
            cursor.alpha = 0.6;

            Global.map.objContainer.addObject(cursor, ObjectContainer.LOWER);

            addEventListener(Event.ADDED_TO_STAGE, onAddedToStage);
            addEventListener(MouseEvent.DOUBLE_CLICK, onMouseDoubleClick);
            addEventListener(MouseEvent.CLICK, onMouseStop, true);
            addEventListener(MouseEvent.MOUSE_MOVE, onMouseMove);
            addEventListener(MouseEvent.MOUSE_OVER, onMouseStop);
            addEventListener(MouseEvent.MOUSE_DOWN, onMouseDown);

            Global.gameContainer.setOverlaySprite(this);
        }
		
		public function getTargetObject(): SimpleGameObject
		{
			return highlightedObj;
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

			if (tooltip) tooltip.hide();

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

			if (highlightedObj == null) 
				return;

			if (onAccept != null)
				onAccept(this);
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

                cursor.x = cursor.primaryPosition.x = pos.x;
                cursor.y = cursor.primaryPosition.y = pos.y;

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
			
			if (tooltip) {
				tooltip.hide();
				tooltip = null;			
			}						

            cursor.visible = true;

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

            cursor.visible = false;

			gameObj.setHighlighted(true);
			highlightedObj = gameObj;

			var targetMapDistance: Position = gameObj.primaryPosition.toPosition();
			var distance: int = TileLocator.distance(city.primaryPosition.x, city.primaryPosition.y, 1, targetMapDistance.x, targetMapDistance.y, 1);
			var timeAwayInSeconds: int = Formula.moveTimeTotal(city, troopSpeed, distance, false);			

			if (Constants.debug)
				Global.gameContainer.message.showMessage("Speed [" +troopSpeed+"] Distance [" + distance + "] in " + timeAwayInSeconds + " sec("+DateUtil.formatTime(timeAwayInSeconds)+")");
			else
				Global.gameContainer.message.showMessage(StringHelper.localize("REINFORCE_DISTANCE_MESSAGE", DateUtil.niceTime(timeAwayInSeconds)));
		}
	}

}


package src.UI.Cursors {
	import fl.lang.*;
	import flash.display.*;
	import flash.events.*;
	import flash.geom.*;
	import mx.utils.*;
	import src.*;
	import src.Map.*;
	import src.Objects.*;
	import src.Objects.Effects.*;
	import src.Objects.Factories.*;
	import src.Objects.Stronghold.*;
	import src.Objects.Troop.*;
	import src.UI.Components.*;
	import src.UI.Tooltips.*;
	import src.Util.*;

	public class GroundAttackCursor extends MovieClip implements IDisposable
	{
		private var objX: int;
		private var objY: int;

		private var originPoint: Point;

		private var cursor: GroundCircle;

		private var tiles: Array = new Array();

		private var troop: TroopStub;
		private var troopSpeed: Number;
		private var city: City;
		private var mode: int;
		
		private var onAccept: Function;

		private var highlightedObj: SimpleGameObject;

		private var tooltip: StructureTooltip;

		public function GroundAttackCursor(city: City, onAccept: Function, troop: TroopStub = null):void
		{
			doubleClickEnabled = true;

			this.troop = troop;
			this.city = city;
			this.mode = mode;
			this.onAccept = onAccept;
			
			if (troop) {
				troopSpeed = troop.getSpeed(city);
			}

			Global.map.selectObject(null);
			Global.map.objContainer.resetObjects();

			var size: int = Formula.troopRadius(troop);

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
			if (Point.distance(MapUtil.getPointWithZoomFactor(event.stageX, event.stageY), originPoint) > 4) return;

			event.stopImmediatePropagation();

			if (highlightedObj == null) 
				return;
			
			if (onAccept != null)
				onAccept(this);
		}

		public function onMouseDown(event: MouseEvent):void
		{
			originPoint = MapUtil.getPointWithZoomFactor(event.stageX, event.stageY);
		}

		public function onMouseMove(event: MouseEvent):void
		{
			if (event.buttonDown) return;

			var mousePos: Point = MapUtil.getPointWithZoomFactor(event.stageX, event.stageY);
			moveTo(mousePos.x, mousePos.y);
		}

		public function moveTo(x: int, y: int):void
		{
			var pos: Point = MapUtil.getActualCoord(Global.gameContainer.camera.x + Math.max(x, 0), Global.gameContainer.camera.y + Math.max(y, 0));

			if (pos.x != objX || pos.y != objY)
			{
				Global.map.objContainer.removeObject(cursor, ObjectContainer.LOWER);

				objX = pos.x;
				objY = pos.y;

				cursor.objX = objX;
				cursor.objY = objY;

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

			var objects: Array = Global.map.regions.getObjectsAt(objX, objY, [StructureObject, Stronghold, BarbarianTribe]);

			if (objects.length == 0) {
				Global.gameContainer.message.showMessage(StringHelper.localize("ATTACK_CHOOSE_TARGET"));
				return;
			}
			
			var gameObj: SimpleGameObject = objects[0];
			
			if (gameObj is StructureObject) {
				var structObj: StructureObject = gameObj as StructureObject;		

				if (Global.gameContainer.map.cities.get(structObj.cityId)) {
					Global.gameContainer.message.showMessage(StringHelper.localize("ATTACK_SELF_ERROR"));
					return;
				}
				
				if (ObjectFactory.isType("Unattackable", structObj.type)) {
					Global.gameContainer.message.showMessage(StringHelper.localize("ATTACK_STRUCTURE_UNATTACKABLE"));
					return;
				}
				
				if (structObj.level == 0) {
					Global.gameContainer.message.showMessage(StringHelper.localize("ATTACK_STRUCTURE_BEING_BUILT"));
					return;
				}			
				
				if (structObj.level == 1 && ObjectFactory.isType("Undestroyable", structObj.type)) {
					Global.gameContainer.message.showMessage(StringHelper.localize("ATTACK_STRUCTURE_UNDESTROYABLE"));
					return;
				}
				
				tooltip = new StructureTooltip(structObj, StructureFactory.getPrototype(structObj.type, structObj.level));
				tooltip.show(structObj);				
			}
			else if (gameObj is Stronghold) {
				var strongholdObj: Stronghold = gameObj as Stronghold;
				
				if (!Constants.tribe.isInTribe()) {
					Global.gameContainer.message.showMessage(StringHelper.localize("ATTACK_STRONGHOLD_NO_TRIBESMAN"));
					return;
				}
				
				if (Constants.tribe.isInTribe(strongholdObj.tribeId)) {
					Global.gameContainer.message.showMessage(StringHelper.localize("ATTACK_OWN_STRONGHOLD"));
					return;
				}
			}
		
			gameObj.setHighlighted(true);
			highlightedObj = gameObj;
			
			var targetMapDistance: Point = MapUtil.getMapCoord(gameObj.objX, gameObj.objY);
			var distance: int = city.MainBuilding.distance(targetMapDistance.x, targetMapDistance.y);
			var timeAwayInSeconds: int = Formula.moveTimeTotal(city, troopSpeed, distance, true);
			if (Constants.debug)
				Global.gameContainer.message.showMessage("Speed [" +troopSpeed+"] Distance [" + distance + "] in " + timeAwayInSeconds + " sec("+Util.formatTime(timeAwayInSeconds)+")");
			else
				Global.gameContainer.message.showMessage(StringHelper.localize("ATTACK_DISTANCE_MESSAGE", Util.niceTime(timeAwayInSeconds)));
		}
	}

}


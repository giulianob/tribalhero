package src.UI.Cursors {
	import flash.display.*;
	import flash.events.*;
	import flash.geom.*;
	import src.*;
	import src.Map.*;
	import src.Objects.*;
	import src.Objects.Effects.*;
	import src.Objects.Factories.*;
	import src.Objects.Troop.*;
	import src.UI.Components.*;
	import src.UI.Tooltips.*;
	import src.Util.*;

	public class GroundReinforceCursor extends MovieClip implements IDisposable
	{
		private var objX: int;
		private var objY: int;

		private var originPoint: Point;

		private var cursor: GroundCircle;

		private var tiles: Array = new Array();

		private var troop: TroopStub;
		private var city: City;

		private var troopSpeed: int;

		private var highlightedObj: GameObject;

		private var tooltip: StructureTooltip;
		
		private var onAccept: Function;

		public function GroundReinforceCursor(onAccept: Function, troop: TroopStub):void
		{
			doubleClickEnabled = true;

			this.troop = troop;
			this.city = Global.gameContainer.selectedCity;
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
		
		public function getTargetObject(): GameObject
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

			var objects: Array = Global.map.regions.getObjectsAt(objX, objY, StructureObject);

			if (objects.length == 0) return;

			var gameObj: StructureObject = objects[0];

			if (gameObj.objectId != 1) return;

			if (onAccept != null)
				onAccept(this);
		}

		public function onMouseDown(event: MouseEvent):void
		{
			originPoint = MapUtil.getPointWithZoomFactor(event.stageX, event.stageY);
		}

		public function onMouseMove(event: MouseEvent):void
		{
			if (event.buttonDown) {
				return;
			}

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

				cursor.setX(objX);
				cursor.setY(objY);

				cursor.moveWithCamera(Global.gameContainer.camera);

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

			var objects: Array = Global.map.regions.getObjectsAt(objX, objY);

			if (tooltip) tooltip.hide();
			tooltip = null;
			
			if (objects.length == 0 || objects[0].objectId != 1) {
				Global.gameContainer.message.showMessage("Choose a town center to defend");								
				return;
			}

			var gameObj: SimpleGameObject = objects[0];
			
			if (highlightedObj == gameObj) return;

			var structObj: StructureObject = gameObj as StructureObject;
			structObj.setHighlighted(true);
			highlightedObj = (gameObj as GameObject);

			var targetMapDistance: Point = MapUtil.getMapCoord(structObj.getX(), structObj.getY());
			var distance: int = city.MainBuilding.distance(targetMapDistance.x, targetMapDistance.y);
			var timeAwayInSeconds: int = Formula.moveTimeTotal(city, troopSpeed, distance, false);
			
			tooltip = new StructureTooltip(structObj, StructureFactory.getPrototype(structObj.type, structObj.level));
			tooltip.show(structObj);

			Global.gameContainer.message.showMessage("About " + Util.niceTime(timeAwayInSeconds) + " away. Double click to defend.");
		}
	}

}


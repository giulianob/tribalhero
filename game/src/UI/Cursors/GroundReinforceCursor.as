package src.UI.Cursors {
	import flash.display.MovieClip;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import flash.geom.Point;
	import src.Constants;
	import src.Global;
	import src.Map.City;
	import src.Objects.Effects.Formula;
	import src.Objects.GameObject;
	import src.Objects.ObjectContainer;
	import src.Objects.SimpleGameObject;
	import src.Map.MapUtil;
	import src.Objects.IDisposable;
	import src.Objects.StructureObject;
	import src.UI.Components.GroundCircle;
	import src.Util.Util;
	import src.Objects.Troop.*;

	public class GroundReinforceCursor extends MovieClip implements IDisposable
	{
		private var objX: int;
		private var objY: int;

		private var originPoint: Point;

		private var cursor: GroundCircle;

		private var tiles: Array = new Array();

		private var troop: TroopStub;
		private var city: City;

		private var highlightedObj: GameObject;

		public function GroundReinforceCursor() {

		}

		public function init(troop: TroopStub, cityId: int):void
		{
			doubleClickEnabled = true;

			this.troop = troop;
			this.city = Global.map.cities.get(cityId);

			Global.map.selectObject(null);
			Global.map.objContainer.resetObjects();

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
			if (Point.distance(new Point(event.stageX, event.stageY), originPoint) > 4)
			return;

			event.stopImmediatePropagation();

			var objects: Array = Global.map.regions.getObjectsAt(objX, objY);

			if (objects.length == 0) return;

			var gameObj: SimpleGameObject = objects[0];

			if (gameObj.objectId != 1) return;

			Global.mapComm.Troop.troopReinforce(city.id, gameObj.cityId, troop);

			Global.gameContainer.setOverlaySprite(null);
			Global.gameContainer.setSidebar(null);
			Global.map.selectObject(null);
		}

		public function onMouseDown(event: MouseEvent):void
		{
			originPoint = new Point(event.stageX, event.stageY);
		}

		public function onMouseMove(event: MouseEvent):void
		{
			if (event.buttonDown) {
				return;
			}

			moveTo(event.stageX, event.stageY);
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

			if (objects.length == 0 || objects[0].objectId != 1) {
				Global.gameContainer.message.showMessage("Choose a town center to defend");
				return;
			}

			var gameObj: SimpleGameObject = objects[0];

			var structObj: StructureObject = gameObj as StructureObject;
			structObj.setHighlighted(true);
			highlightedObj = (gameObj as GameObject);

			var targetMapDistance: Point = MapUtil.getMapCoord(structObj.getX(), structObj.getY());
			var distance: int = city.MainBuilding.distance(targetMapDistance.x, targetMapDistance.y);
			var timeAwayInSeconds: int = Math.max(1, Formula.moveTime(troop.getSpeed()) * Constants.secondsPerUnit * distance);

			Global.gameContainer.message.showMessage("About " + Util.niceTime(timeAwayInSeconds) + " away. Double click to defend.");
		}
	}

}


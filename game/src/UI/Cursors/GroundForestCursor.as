package src.UI.Cursors {
	import flash.display.MovieClip;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import flash.geom.Point;
	import src.Global;
	import src.Map.City;
	import src.Objects.Factories.ObjectFactory;
	import src.Objects.Forest;
	import src.Objects.GameObject;
	import src.Objects.ObjectContainer;
	import src.Objects.SimpleGameObject;
	import src.Map.MapUtil;
	import src.Objects.IDisposable;
	import src.UI.Components.GroundCircle;
	import src.Objects.Troop.*;
	import src.UI.Sidebars.CursorCancel.CursorCancelSidebar;

	public class GroundForestCursor extends MovieClip implements IDisposable
	{
		private var objX: int;
		private var objY: int;

		private var originPoint: Point;

		private var cursor: GroundCircle;

		private var tiles: Array = new Array();

		private var city: City;

		private var highlightedObj: GameObject;

		private var onAccept: Function;

		public function GroundForestCursor(cityId: int, onAccept: Function):void
		{
			this.onAccept = onAccept;
			doubleClickEnabled = true;

			this.city = Global.map.cities.get(cityId);

			Global.map.selectObject(null);
			Global.map.objContainer.resetObjects();

			var size: int = 0;

			cursor = new GroundCircle(size);
			cursor.alpha = 0.6;

			Global.map.objContainer.addObject(cursor, ObjectContainer.LOWER);
			
			var sidebar: CursorCancelSidebar = new CursorCancelSidebar(null);
			src.Global.gameContainer.setSidebar(sidebar);			

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
			if (Point.distance(MapUtil.getPointWithZoomFactor(event.stageX, event.stageY), originPoint) > 4) return;

			event.stopImmediatePropagation();

			var objects: Array = Global.map.regions.getObjectsAt(objX, objY);

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

			var objects: Array = Global.map.regions.getObjectsAt(objX, objY);

			if (objects.length == 0 || !(objects[0] is Forest)) {
				Global.gameContainer.message.showMessage("Choose a forest to gather wood from.");
				return;
			}

			var gameObj: SimpleGameObject = objects[0];

			var forestObj: Forest = gameObj as Forest;
			forestObj.setHighlighted(true);
			highlightedObj = (gameObj as GameObject);

			var targetMapDistance: Point = MapUtil.getMapCoord(forestObj.objX, forestObj.objY);
			var distance: int = city.MainBuilding.distance(targetMapDistance.x, targetMapDistance.y);

			Global.gameContainer.message.showMessage("Double click this forest to gather wood.");
		}
	}

}


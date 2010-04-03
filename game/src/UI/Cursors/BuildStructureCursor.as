
package src.UI.Cursors {
	import flash.geom.ColorTransform;
	import flash.geom.Point;
	import src.Global;
	import src.Map.City;
	import src.Map.Map;
	import src.Map.MapUtil;
	import flash.display.MovieClip;
	import flash.events.MouseEvent;
	import src.Objects.Factories.*;
	import src.Objects.GameObject;
	import src.Objects.IDisposable;
	import src.Objects.ObjectContainer;
	import src.Objects.Prototypes.StructurePrototype;
	import src.Objects.SimpleObject;
	import src.UI.Components.GroundCallbackCircle;
	import src.UI.Components.GroundCircle;
	import src.UI.Sidebars.CursorCancel.CursorCancelSidebar;

	public class BuildStructureCursor extends MovieClip implements IDisposable
	{
		private var map: Map;
		private var objX: int;
		private var objY: int;
		private var city: City;

		private var originPoint: Point;

		private var cursor: SimpleObject;
		private var rangeCursor: GroundCircle;
		private var buildableArea: GroundCallbackCircle;
		private var structPrototype: StructurePrototype;
		private var parentObj: GameObject;
		private var type: int;
		private var level: int;
		private var tilerequirement: String;

		public function BuildStructureCursor() { }

		public function init(map: Map, type: int, level: int, tilerequirement: String, parentObject: GameObject):void
		{
			doubleClickEnabled = true;

			this.parentObj = parentObject;
			this.type = type;
			this.level = level;
			this.tilerequirement = tilerequirement;
			this.map = map;

			city = map.cities.get(parentObj.cityId);

			src.Global.gameContainer.setOverlaySprite(this);
			map.selectObject(null);

			structPrototype = StructureFactory.getPrototype(type, level);
			cursor = StructureFactory.getSimpleObject(type, level);

			if (StructureFactory.getSimpleObject(type, level) == null)
			{
				trace("Missing building cursor " + type);
				return;
			}

			cursor.alpha = 0.7;

			rangeCursor = new GroundCircle(structPrototype.radius, true, new ColorTransform(1.0, 1.0, 1.0, 1.0, 236, 88, 0));
			rangeCursor.alpha = 0.6;

			buildableArea = new GroundCallbackCircle(city.radius - 1, validateTileCallback);
			buildableArea.alpha = 0.3;
			var point: Point = MapUtil.getScreenCoord(city.MainBuilding.x, city.MainBuilding.y);
			buildableArea.setX(point.x); buildableArea.setY(point.y);
			buildableArea.moveWithCamera(src.Global.gameContainer.camera);
			map.objContainer.addObject(buildableArea, ObjectContainer.LOWER);

			var sidebar: CursorCancelSidebar = new CursorCancelSidebar(parentObj);
			src.Global.gameContainer.setSidebar(sidebar);

			addEventListener(MouseEvent.DOUBLE_CLICK, onMouseDoubleClick);
			addEventListener(MouseEvent.CLICK, onMouseStop, true);
			addEventListener(MouseEvent.MOUSE_MOVE, onMouseMove);
			addEventListener(MouseEvent.MOUSE_OVER, onMouseStop);
			addEventListener(MouseEvent.MOUSE_DOWN, onMouseDown);

			src.Global.gameContainer.message.showMessage("Double click on a green square to build a " + structPrototype.getName().toLowerCase() + ".");
		}

		public function dispose():void
		{
			src.Global.gameContainer.message.hide();

			if (cursor != null)
			{
				if (cursor.stage != null) map.objContainer.removeObject(cursor);
				if (rangeCursor.stage != null) map.objContainer.removeObject(rangeCursor, ObjectContainer.LOWER);
				if (buildableArea.stage != null) map.objContainer.removeObject(buildableArea, ObjectContainer.LOWER);
			}
		}

		private function showCursors() : void {
			if (cursor) cursor.visible = true;
			if (rangeCursor) rangeCursor.visible = true;
		}

		private function hideCursors() : void {
			if (cursor) cursor.visible = false;
			if (rangeCursor) rangeCursor.visible = false;
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

			var pos: Point = MapUtil.getMapCoord(objX, objY);
			Global.mapComm.Object.buildStructure(parentObj.cityId, parentObj.objectId, type, pos.x, pos.y);

			src.Global.gameContainer.setOverlaySprite(null);
			src.Global.gameContainer.setSidebar(null);
			map.selectObject(parentObj, false);
		}

		public function onMouseDown(event: MouseEvent):void
		{
			originPoint = new Point(event.stageX, event.stageY);
		}

		public function onMouseMove(event: MouseEvent) : void
		{
			if (event.buttonDown)
			return;

			var pos: Point = MapUtil.getActualCoord(src.Global.gameContainer.camera.x + Math.max(event.stageX, 0), src.Global.gameContainer.camera.y + Math.max(event.stageY, 0));

			if (pos.x != objX || pos.y != objY)
			{
				objX = pos.x;
				objY = pos.y;

				//Range cursor
				if (rangeCursor.stage != null) map.objContainer.removeObject(rangeCursor, ObjectContainer.LOWER);
				rangeCursor.setX(objX); rangeCursor.setY(objY);
				rangeCursor.moveWithCamera(src.Global.gameContainer.camera);
				map.objContainer.addObject(rangeCursor, ObjectContainer.LOWER);

				//Object cursor
				if (cursor.stage != null) map.objContainer.removeObject(cursor);
				cursor.setX(objX); cursor.setY(objY);
				cursor.moveWithCamera(src.Global.gameContainer.camera);
				map.objContainer.addObject(cursor);
				validateBuilding();
			}
		}

		private function validateTileCallback(x: int, y: int, isCenter: Boolean) : * {

			// Get the screen position of the main building then we'll add the current tile x and y to get the point of this tile on the screen
			var point: Point = MapUtil.getScreenCoord(city.MainBuilding.x, city.MainBuilding.y);

			// Get the tile type
			var mapPos: Point = MapUtil.getMapCoord(point.x + x, point.y + y);
			var tileType: int = map.regions.getTileAt(mapPos.x, mapPos.y);

			if (!structPrototype.validateLayout(map, city, point.x + x, point.y + y)) {
				return false;
			} else if (tilerequirement == "" && !ObjectFactory.isType("TileBuildable", tileType)) {
				return false;
			}
			else if (tilerequirement != "" && !ObjectFactory.isType(tilerequirement, tileType)) {
				return false;
			}

			return new ColorTransform(1.0, 1.0, 1.0, 1.0, 0, 100);
		}

		public function validateBuilding():void
		{
			var msg: XML;

			var city: City = map.cities.get(parentObj.cityId);
			var mapObjPos: Point = MapUtil.getMapCoord(objX, objY);
			var tileType: int = map.regions.getTileAt(mapObjPos.x, mapObjPos.y);

			// Check for valid layout
			if (!structPrototype.validateLayout(map, city, objX, objY))
			{
				hideCursors();
				return;
			}
			// Check if cursor is inside city walls
			else if (city != null && MapUtil.distance(city.MainBuilding.x, city.MainBuilding.y, mapObjPos.x, mapObjPos.y) >= city.radius) {
				hideCursors();
				return;
			}
			else if (tilerequirement == "" && !ObjectFactory.isType("TileBuildable", tileType)) {
				hideCursors();
				return;
			}
			else if (tilerequirement != "" && !ObjectFactory.isType(tilerequirement, tileType)) {
				hideCursors();
				return;
			}

			showCursors();
		}
	}
}


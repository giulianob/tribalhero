
package src.UI.Cursors {
	import flash.geom.ColorTransform;
	import flash.geom.Point;
	import src.Constants;
	import src.Global;
	import src.Map.City;
	import src.Map.CityObject;
	import src.Map.Map;
	import src.Map.MapUtil;
	import flash.display.MovieClip;
	import flash.events.MouseEvent;
	import src.Map.RoadPathFinder;
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

		private var hasBuildableArea: Boolean;
		private var hasRoadNearby: Boolean;

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

			if (!hasRoadNearby) {
				src.Global.gameContainer.message.showMessage("This building must be connected to a road and there are no roads available. Build extra roads first then try again.");
			} else if (!hasBuildableArea) {
				src.Global.gameContainer.message.showMessage("There are no spaces available to build on.");
			} else {
				src.Global.gameContainer.message.showMessage("Double click on a green square to build a " + structPrototype.getName().toLowerCase() + ".");
			}
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
			if (Point.distance(MapUtil.getPointWithZoomFactor(event.stageX, event.stageY), originPoint) > city.radius) return;

			event.stopImmediatePropagation();

			var pos: Point = MapUtil.getMapCoord(objX, objY);
			Global.mapComm.Object.buildStructure(parentObj.cityId, parentObj.objectId, type, pos.x, pos.y);

			src.Global.gameContainer.setOverlaySprite(null);
			src.Global.gameContainer.setSidebar(null);
			map.selectObject(parentObj, false);
		}

		public function onMouseDown(event: MouseEvent):void
		{
			originPoint = MapUtil.getPointWithZoomFactor(event.stageX, event.stageY);
		}

		public function onMouseMove(event: MouseEvent) : void
		{
			if (event.buttonDown) return;

			var mousePos: Point = MapUtil.getPointWithZoomFactor(Math.max(0, event.stageX), Math.max(0, event.stageY));
			var pos: Point = MapUtil.getActualCoord(src.Global.gameContainer.camera.x + mousePos.x, src.Global.gameContainer.camera.y + mousePos.y);

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

		private function validateTile(screenPos: Point) : Boolean {
			// Get the tile type
			var mapPos: Point = MapUtil.getMapCoord(screenPos.x, screenPos.y);
			var tileType: int = map.regions.getTileAt(mapPos.x, mapPos.y);

			if (Constants.debug >= 4) {
				trace("***");
				trace("Validating:" + screenPos.x + "," + screenPos.y);
				trace("Callback pos:" + x + "," + y);
				trace("mapPos is" + mapPos.x + "," + mapPos.y);
				trace("Tile type is: " + tileType);
			}

			var requiredRoad: Boolean = !ObjectFactory.isType("NoRoadRequired", type);

			// Set flag so that buildings that dont require roads dont screw up the on screen msg
			if (!requiredRoad) this.hasRoadNearby = true;

			// Validate any layouts
			var builder: CityObject = city.objects.get(parentObj.objectId);
			if (!structPrototype.validateLayout(builder, city, screenPos.x, screenPos.y)) {
				return false;
			}

			// Check for tile requirement
			if (tilerequirement == "" && !RoadPathFinder.isRoad(tileType) && !ObjectFactory.isType("TileBuildable", tileType)) return false;

			// Check for tile requirement
			if (tilerequirement != "" && !ObjectFactory.isType(tilerequirement, tileType)) return false;

			var buildingOnRoad: Boolean = RoadPathFinder.isRoad(tileType);

			if (!requiredRoad) {
				// Don't allow structures that don't need roads to be built on top of roads
				if (buildingOnRoad) return false;

				this.hasBuildableArea = true;
				return true;
			}

			// Keep non road related checks above this
			// Check for road requirement
			if (buildingOnRoad) {
				var breaksPath: Boolean = false;
				for each(var cityObject: CityObject in city.objects.each()) {
					if (cityObject.x == city.MainBuilding.x && cityObject.y == city.MainBuilding.y) continue;
					if (ObjectFactory.isType("NoRoadRequired", cityObject.type)) continue;

					if (!RoadPathFinder.hasPath(new Point(cityObject.x, cityObject.y), new Point(city.MainBuilding.x, city.MainBuilding.y), city, mapPos)) {
						breaksPath = true;
						break;
					}
				}

				if (breaksPath) return false;

				// Make sure all neighbors have a different path
				var allNeighborsHaveOtherPaths: Boolean = true;
				MapUtil.foreach_object(mapPos.x, mapPos.y, 1, function(x1: int, y1: int, custom: *) : Boolean
				{
					if (MapUtil.radiusDistance(mapPos.x, mapPos.y, x1, y1) != 1) return true;

					if (city.MainBuilding.x == x1 && city.MainBuilding.y == y1) return true;

					if (RoadPathFinder.isRoadByMapPosition(x1, y1)) {
						if (!RoadPathFinder.hasPath(new Point(x1, y1), new Point(city.MainBuilding.x, city.MainBuilding.y), city, mapPos)) {
							allNeighborsHaveOtherPaths = false;
							return false;
						}
					}

					return true;
				}, false, null);

				if (!allNeighborsHaveOtherPaths) return false;
			}

			var hasRoad: Boolean = false;

			MapUtil.foreach_object(mapPos.x, mapPos.y, 1, function(x1: int, y1: int, custom: *) : Boolean
			{
				if (MapUtil.radiusDistance(mapPos.x, mapPos.y, x1, y1) != 1) return true;

				var structure: CityObject = city.getStructureAt(new Point(x1, y1));

				var hasStructure: Boolean = structure != null;

				// Make sure we have a road around this building
				if (!hasRoad && !hasStructure && RoadPathFinder.isRoadByMapPosition(x1, y1)) {
					// If we are building on road, we need to check that all neighbor tiles have another connection to the main building
					if (!buildingOnRoad || RoadPathFinder.hasPath(new Point(x1, y1), new Point(city.MainBuilding.x, city.MainBuilding.y), city, mapPos)) {
						hasRoad = true;
					}
				}

				return true;
			}, false, null);

			// Set global variable to identify if we have buildable road. This is used for the on screen message
			if (hasRoad) this.hasRoadNearby = true;

			if (!hasRoad) return false

			//Set other global variable to identify we have a buildable spot.
			hasBuildableArea = true;

			return true;
		}

		private function validateTileCallback(x: int, y: int, isCenter: Boolean) : * {

			// Get the screen position of the main building then we'll add the current tile x and y to get the point of this tile on the screen
			var point: Point = MapUtil.getScreenCoord(city.MainBuilding.x, city.MainBuilding.y);

			if (!validateTile(new Point(point.x + x, point.y + y))) return false;

			return new ColorTransform(1.0, 1.0, 1.0, 1.0, 0, 100);
		}

		public function validateBuilding():void
		{
			var msg: XML;

			var city: City = map.cities.get(parentObj.cityId);
			var mapObjPos: Point = MapUtil.getMapCoord(objX, objY);

			// Check if cursor is inside city walls
			if (city != null && MapUtil.distance(city.MainBuilding.x, city.MainBuilding.y, mapObjPos.x, mapObjPos.y) >= city.radius) {
				hideCursors();
			}
			else if (!validateTile(new Point(objX, objY))) {
				hideCursors();
			}
			else {
				showCursors();
			}
		}
	}
}


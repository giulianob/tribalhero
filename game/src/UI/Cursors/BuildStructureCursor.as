
package src.UI.Cursors {
    import flash.display.*;
    import flash.events.*;
    import flash.geom.*;

    import src.*;
    import src.Map.*;
    import src.Objects.*;
    import src.Objects.Factories.*;
    import src.Objects.Prototypes.*;
    import src.UI.Components.*;
    import src.UI.Sidebars.CursorCancel.*;
    import src.Util.*;

    public class BuildStructureCursor extends MovieClip implements IDisposable
	{		
		private var objPosition: ScreenPosition = new ScreenPosition();

		private var city: City;

		private var originPoint: Point;

		private var cursor: SimpleObject;
		private var rangeCursor: GroundCircle;
		private var buildableArea: GroundCallbackCircle;
		private var structPrototype: StructurePrototype;
		private var parentObj: SimpleGameObject;
		private var type: int;
		private var level: int;
		private var tilerequirement: String;

		private var hasBuildableArea: Boolean;
		private var hasRoadNearby: Boolean;

		public function BuildStructureCursor(type: int, level: int, tilerequirement: String, parentObject: SimpleGameObject):void
		{
			doubleClickEnabled = true;

			this.parentObj = parentObject;
			this.type = type;
			this.level = level;
			this.tilerequirement = tilerequirement;

			city = Global.map.cities.get(parentObj.groupId);

			Global.gameContainer.setOverlaySprite(this);
			Global.map.selectObject(null);

			structPrototype = StructureFactory.getPrototype(type, level);
			cursor = StructureFactory.getSimpleObject(type, level, 0, 0, structPrototype.size);

			if (cursor == null)
			{
				Util.log("Missing building cursor " + type);
				return;
			}

			cursor.alpha = 0.7;

			rangeCursor = new GroundCircle(structPrototype.radius, true, new ColorTransform(1.0, 1.0, 1.0, 1.0, 236, 88, 0));
			rangeCursor.alpha = 0.6;

			buildableArea = new GroundCallbackCircle(city.radius - 1, validateTileCallback);
			buildableArea.alpha = 0.3;
			var point: Point = TileLocator.getScreenCoord(city.MainBuilding.x, city.MainBuilding.y);
			buildableArea.objX = point.x; 
			buildableArea.objY = point.y;

			Global.map.objContainer.addObject(buildableArea, ObjectContainer.LOWER);

			var sidebar: CursorCancelSidebar = new CursorCancelSidebar(parentObj);
			Global.gameContainer.setSidebar(sidebar);

			addEventListener(MouseEvent.DOUBLE_CLICK, onMouseDoubleClick);
			addEventListener(MouseEvent.CLICK, onMouseStop, true);
			addEventListener(MouseEvent.MOUSE_MOVE, onMouseMove);
			addEventListener(MouseEvent.MOUSE_OVER, onMouseStop);
			addEventListener(MouseEvent.MOUSE_DOWN, onMouseDown);

			if (!hasRoadNearby) {
				Global.gameContainer.message.showMessage("This building must be connected to a road and there are no roads available. Build roads first by using your Town Center then try again.");
			} else if (!hasBuildableArea) {
				Global.gameContainer.message.showMessage("There are no spaces available to build on.");
			} else {
				Global.gameContainer.message.showMessage("Double click on a green square to build a " + structPrototype.getName().toLowerCase() + ".");
			}
		}

		public function dispose():void
		{
			Global.gameContainer.message.hide();

			if (cursor != null)
			{
				if (cursor.stage != null) Global.map.objContainer.removeObject(cursor);
				if (rangeCursor.stage != null) Global.map.objContainer.removeObject(rangeCursor, ObjectContainer.LOWER);
				if (buildableArea.stage != null) Global.map.objContainer.removeObject(buildableArea, ObjectContainer.LOWER);
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
			if (Point.distance(TileLocator.getPointWithZoomFactor(event.stageX, event.stageY), originPoint) > city.radius) return;

			event.stopImmediatePropagation();

			var mapPos: Position = objPosition.toPosition();
			Global.mapComm.Objects.buildStructure(parentObj.groupId, parentObj.objectId, type, level, mapPos.x, mapPos.y);

			Global.gameContainer.setOverlaySprite(null);
			Global.gameContainer.setSidebar(null);
			Global.map.selectObject(parentObj, false);
		}

		public function onMouseDown(event: MouseEvent):void
		{
			originPoint = TileLocator.getPointWithZoomFactor(event.stageX, event.stageY);
		}

		public function onMouseMove(event: MouseEvent) : void
		{
			if (event.buttonDown) return;

			var mousePos: Point = TileLocator.getPointWithZoomFactor(Math.max(0, event.stageX), Math.max(0, event.stageY));
			var pos: ScreenPosition = TileLocator.getActualCoord(Global.gameContainer.camera.x + mousePos.x, Global.gameContainer.camera.y + mousePos.y);

			if (!pos.equals(objPosition))
			{
				objPosition = pos;
			
				if (rangeCursor.stage != null) Global.map.objContainer.removeObject(rangeCursor, ObjectContainer.LOWER);
				if (cursor.stage != null) Global.map.objContainer.removeObject(cursor);
				
				rangeCursor.objX = pos.x;
				rangeCursor.objY = pos.y;
				
				cursor.objX = pos.x;
				cursor.objY = pos.y;
				
				if (validateBuilding()) {
					Global.map.objContainer.addObject(rangeCursor, ObjectContainer.LOWER);
					Global.map.objContainer.addObject(cursor);
				}
			}
		}

		private function validateTile(position: ScreenPosition) : Boolean {
            var mapPosition: Position = position.toPosition();

			// Get the tile type
			var tileType: int = Global.map.regions.getTileAt(mapPosition.x, mapPosition.y);

			if (Constants.debug >= 4) {
				Util.log("***");
				Util.log("Callback pos:" + x + "," + y);
				Util.log("mapPos is" + mapPosition.x + "," + mapPosition.y);
				Util.log("Tile type is: " + tileType);
			}

			var requiredRoad: Boolean = !ObjectFactory.isType("NoRoadRequired", type);

			// Set flag so that buildings that dont require roads dont screw up the on screen msg
			if (!requiredRoad) {
                this.hasRoadNearby = true;
            }

			// Validate any layouts
			var builder: CityObject = city.objects.get(parentObj.objectId);
			if (!structPrototype.validateLayout(builder, city, mapPosition)) {
				return false;
			}

			// Check for tile requirement
			if (tilerequirement == "" && !RoadPathFinder.isRoad(tileType) && !ObjectFactory.isType("TileBuildable", tileType)) return false;

			// Check for tile requirement
			if (tilerequirement != "" && !ObjectFactory.isType(tilerequirement, tileType)) return false;

            var hasRoad: Boolean = RoadPathFinder.CanBuild(mapPosition, city, requiredRoad);

            hasRoadNearby = !requiredRoad || hasRoad;

            if (!hasRoad) {
                return false;
            }

			hasBuildableArea = true;
			return true;
		}

        private function validateTileCallback(x: int, y: int): * {

            // Get the screen position of the main building then we'll add the current tile x and y to get the point of this tile on the screen
			var point: Point = TileLocator.getScreenCoord(city.MainBuilding.x, city.MainBuilding.y);

			if (!validateTile(new ScreenPosition(point.x + x, point.y + y))) return new ColorTransform(1, 1, 1, 0.5, 255, 215);

			return new ColorTransform(1.0, 1.0, 1.0, 1.0, 0, 100);
		}

		public function validateBuilding():Boolean
		{

            var city: City = Global.map.cities.get(parentObj.groupId);
			var mapObjPos: Position = objPosition.toPosition();

			// Check if cursor is inside city walls
			if (city != null && TileLocator.distance(city.MainBuilding.x, city.MainBuilding.y, mapObjPos.x, mapObjPos.y) >= city.radius) {
				hideCursors();
				return false;
			}
			else if (!validateTile(objPosition)) {
				hideCursors();
				return false;
			}
			
			showCursors();
			return true;
		}
	}
}


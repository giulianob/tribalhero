
package src.UI.Cursors {
	import flash.events.Event;
	import flash.events.TimerEvent;
	import flash.geom.ColorTransform;
	import flash.geom.Point;
	import flash.utils.Timer;
	import src.Global;
	import flash.display.MovieClip;
	import flash.events.MouseEvent;
	import src.Map.*;
	import src.Objects.Factories.*;
	import src.Objects.*;
	import src.UI.Components.*;
	import src.UI.Sidebars.CursorCancel.CursorCancelSidebar;

	public class DestroyRoadCursor extends MovieClip implements IDisposable
	{
		private var objX: int;
		private var objY: int;
		private var city: City;

		private var originPoint: Point;

		private var cursor: SimpleObject;
		private var destroyableArea: GroundCallbackCircle;
		private var parentObj: SimpleGameObject;

		private var redrawLaterTimer: Timer = new Timer(250);

		public function DestroyRoadCursor() { }

		public function init(parentObject: SimpleGameObject):void
		{
			doubleClickEnabled = true;

			this.parentObj = parentObject;

			city = Global.map.cities.get(parentObj.groupId);

			Global.gameContainer.setOverlaySprite(this);
			Global.map.selectObject(null);

			cursor = new GroundCircle(0);
			cursor.alpha = 0.7;

			destroyableArea = new GroundCallbackCircle(city.radius - 1, validateTileCallback);						
			
			destroyableArea.alpha = 0.3;
			var point: Point = MapUtil.getScreenCoord(city.MainBuilding.x, city.MainBuilding.y);
			destroyableArea.objX = point.x; 
			destroyableArea.objY = point.y;
			
			Global.map.objContainer.addObject(destroyableArea, ObjectContainer.LOWER);

			var sidebar: CursorCancelSidebar = new CursorCancelSidebar(parentObj);
			src.Global.gameContainer.setSidebar(sidebar);

			addEventListener(MouseEvent.DOUBLE_CLICK, onMouseDoubleClick);
			addEventListener(MouseEvent.CLICK, onMouseStop, true);
			addEventListener(MouseEvent.MOUSE_MOVE, onMouseMove);
			addEventListener(MouseEvent.MOUSE_OVER, onMouseStop);
			addEventListener(MouseEvent.MOUSE_DOWN, onMouseDown);

			Global.map.regions.addEventListener(RegionList.REGION_UPDATED, update);

			redrawLaterTimer.addEventListener(TimerEvent.TIMER, function(e: Event) : void {
				redrawLaterTimer.stop();
				destroyableArea.redraw();
				validateBuilding();
			});

			src.Global.gameContainer.message.showMessage("Double click on a highlighted road to destroy it. Roads that are not highlighted may not be destroyed.");
		}

		public function update(e: Event = null) : void {
			redrawLaterTimer.stop();
			redrawLaterTimer.start();
		}

		public function dispose():void
		{
			Global.map.regions.removeEventListener(RegionList.REGION_UPDATED, update);

			src.Global.gameContainer.message.hide();

			if (cursor != null)
			{
				if (cursor.stage != null) 
					Global.map.objContainer.removeObject(cursor);
					
				if (destroyableArea.stage != null) 
					Global.map.objContainer.removeObject(destroyableArea, ObjectContainer.LOWER);
			}
		}

		private function showCursors() : void {
			if (cursor) cursor.visible = true;
		}

		private function hideCursors() : void {
			if (cursor) cursor.visible = false;
		}

		public function onMouseStop(event: MouseEvent):void
		{
			event.stopImmediatePropagation();
		}

		public function onMouseDoubleClick(event: MouseEvent):void
		{
			if (!cursor.visible)
				return;
				
			if (Point.distance(MapUtil.getPointWithZoomFactor(event.stageX, event.stageY), originPoint) > city.radius) 
				return;

			event.stopImmediatePropagation();

			var pos: Point = MapUtil.getMapCoord(objX, objY);
			Global.mapComm.Region.destroyRoad(parentObj.groupId, pos.x, pos.y);
		}

		public function onMouseDown(event: MouseEvent):void
		{
			originPoint = MapUtil.getPointWithZoomFactor(event.stageX, event.stageY);
		}

		public function onMouseMove(event: MouseEvent) : void
		{
			if (event.buttonDown) 
				return;

			var mousePos: Point = MapUtil.getPointWithZoomFactor(Math.max(0, event.stageX), Math.max(0, event.stageY));
			var pos: Point = MapUtil.getActualCoord(src.Global.gameContainer.camera.x + mousePos.x, src.Global.gameContainer.camera.y + mousePos.y);

			if (pos.x != objX || pos.y != objY)
			{
				objX = pos.x;
				objY = pos.y;

				//Object cursor
				if (cursor.stage != null) 
					Global.map.objContainer.removeObject(cursor);
					
				cursor.objX = objX;
				cursor.objY = objY;
				
				Global.map.objContainer.addObject(cursor);
				
				validateBuilding();
			}
		}

		private function validateTile(screenPos: Point) : Boolean {
			var mapPos: Point = MapUtil.getMapCoord(screenPos.x, screenPos.y);
			var tileType: int = Global.map.regions.getTileAt(mapPos.x, mapPos.y);

			if (!RoadPathFinder.isRoad(tileType)) 
				return false;

			if (Global.map.regions.getObjectsAt(screenPos.x, screenPos.y, StructureObject).length > 0) 
				return false;

			// Make sure that buildings have a path back to the city without this point				
			var breaksPath: Boolean = false;
			for each(var cityObject: CityObject in city.objects) {
				if (ObjectFactory.getClassType(cityObject.type) != ObjectFactory.TYPE_STRUCTURE) 
					continue;
					
				if (cityObject.x == city.MainBuilding.x && cityObject.y == city.MainBuilding.y) 
					continue;
					
				if (ObjectFactory.isType("NoRoadRequired", cityObject.type)) 
					continue;

				if (!RoadPathFinder.hasPath(new Point(cityObject.x, cityObject.y), new Point(city.MainBuilding.x, city.MainBuilding.y), city, mapPos)) {
					breaksPath = true;
					break;
				}
			}

			if (breaksPath) 
				return false;

			// Make sure all neighbors have a different path
			var allNeighborsHaveOtherPaths: Boolean = true;
			MapUtil.foreach_object(mapPos.x, mapPos.y, 1, function(x1: int, y1: int, custom: *) : Boolean
			{
				if (MapUtil.radiusDistance(mapPos.x, mapPos.y, x1, y1) != 1) 
					return true;

				if (city.MainBuilding.x == x1 && city.MainBuilding.y == y1) 
					return true;
				
				if (RoadPathFinder.isRoadByMapPosition(x1, y1)) {					
					if (!RoadPathFinder.hasPath(new Point(x1, y1), new Point(city.MainBuilding.x, city.MainBuilding.y), city, mapPos)) {
						allNeighborsHaveOtherPaths = false;
						return false;
					}
				}

				return true;
			}, false, null);

			if (!allNeighborsHaveOtherPaths) 
				return false;
			
			return true;
		}

		private function validateTileCallback(x: int, y: int, isCenter: Boolean) : * {

			// Get the screen position of the main building then we'll add the current tile x and y to get the point of this tile on the screen
			var point: Point = MapUtil.getScreenCoord(city.MainBuilding.x, city.MainBuilding.y);

			if (!validateTile(new Point(point.x + x, point.y + y))) 
				return false;

			return new ColorTransform(1.0, 1.0, 1.0, 1.0, 100);
		}

		public function validateBuilding():void
		{
			var msg: XML;

			var city: City = Global.map.cities.get(parentObj.groupId);
			var mapObjPos: Point = MapUtil.getMapCoord(objX, objY);

			// Check if cursor is inside city walls
			if (city != null && MapUtil.distance(city.MainBuilding.x, city.MainBuilding.y, mapObjPos.x, mapObjPos.y) >= city.radius)
				hideCursors();
			
			// Perform other validations
			else if (!validateTile(new Point(objX, objY)))
				hideCursors();
			else
				showCursors();			
		}
	}
}



package src.UI.Cursors {
    import flash.display.MovieClip;
    import flash.events.Event;
    import flash.events.MouseEvent;
    import flash.events.TimerEvent;
    import flash.geom.ColorTransform;
    import flash.geom.Point;
    import flash.utils.Timer;

    import src.Global;
    import src.Map.*;
    import src.Objects.*;
    import src.Objects.Factories.*;
    import src.UI.Components.*;
    import src.UI.Sidebars.CursorCancel.CursorCancelSidebar;

    public class DestroyRoadCursor extends MovieClip implements IDisposable
	{
		private var objPosition: ScreenPosition = new ScreenPosition();

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
			var pos: ScreenPosition = city.primaryPosition.toScreenPosition();
            destroyableArea.x = destroyableArea.primaryPosition.x = pos.x;
            destroyableArea.y = destroyableArea.primaryPosition.y = pos.y;

			Global.map.objContainer.addObject(destroyableArea, ObjectContainer.LOWER);

			var sidebar: CursorCancelSidebar = new CursorCancelSidebar(parentObj);
			Global.gameContainer.setSidebar(sidebar);

			addEventListener(MouseEvent.DOUBLE_CLICK, onMouseDoubleClick);
			addEventListener(MouseEvent.CLICK, onMouseStop, true);
			addEventListener(MouseEvent.MOUSE_MOVE, onMouseMove);
			addEventListener(MouseEvent.MOUSE_OVER, onMouseStop);
			addEventListener(MouseEvent.MOUSE_DOWN, onMouseDown);

			Global.map.regions.addEventListener(RegionManager.REGION_UPDATED, update);

			redrawLaterTimer.addEventListener(TimerEvent.TIMER, function(e: Event) : void {
				redrawLaterTimer.stop();
				destroyableArea.redraw();
				validateBuilding();
			});

			Global.gameContainer.message.showMessage("Double click on a highlighted road to destroy it. Roads that are not highlighted may not be destroyed.");
		}

		public function update(e: Event = null) : void {
			redrawLaterTimer.stop();
			redrawLaterTimer.start();
		}

		public function dispose():void
		{
			Global.map.regions.removeEventListener(RegionManager.REGION_UPDATED, update);

			Global.gameContainer.message.hide();

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
				
			if (Point.distance(TileLocator.getPointWithZoomFactor(event.stageX, event.stageY), originPoint) > city.radius)
				return;

			event.stopImmediatePropagation();

			var mapPos: Position = objPosition.toPosition();
			Global.mapComm.Region.destroyRoad(parentObj.groupId, mapPos.x, mapPos.y);
		}

		public function onMouseDown(event: MouseEvent):void
		{
			originPoint = TileLocator.getPointWithZoomFactor(event.stageX, event.stageY);
		}

		public function onMouseMove(event: MouseEvent) : void
		{
			if (event.buttonDown) 
				return;

			var mousePos: Point = TileLocator.getPointWithZoomFactor(Math.max(0, event.stageX), Math.max(0, event.stageY));
			var pos: ScreenPosition = TileLocator.getActualCoord(
                    Global.gameContainer.camera.currentPosition.x + mousePos.x,
                    Global.gameContainer.camera.currentPosition.y + mousePos.y);

			if (!pos.equals(objPosition))
			{
				objPosition = pos;

				//Object cursor
				if (cursor.stage != null) {
					Global.map.objContainer.removeObject(cursor);
                }

                cursor.x = cursor.primaryPosition.x = pos.x;
                cursor.y = cursor.primaryPosition.y = pos.y;
				
				Global.map.objContainer.addObject(cursor);
				
				validateBuilding();
			}
		}

		private function validateTile(screenPos: ScreenPosition) : Boolean {
            return true;

			var mapPos: Position = screenPos.toPosition();
			var tileType: int = Global.map.regions.getTileAt(mapPos);

			if (!RoadPathFinder.isRoad(tileType)) 
				return false;

			if (Global.map.regions.getObjectsInTile(mapPos, StructureObject).length > 0)
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

				if (!RoadPathFinder.hasPath(new Position(cityObject.x, cityObject.y), new Position(city.MainBuilding.x, city.MainBuilding.y), city, mapPos)) {
					breaksPath = true;
					break;
				}
			}

			if (breaksPath) 
				return false;

			// Make sure all neighbors have a different path
			for each (var position: Position in TileLocator.foreachTile(mapPos.x, mapPos.y, 1, false))
			{
				if (TileLocator.radiusDistance(mapPos.x, mapPos.y, 1, position.x, position.y, 1) != 1)
                {
                    continue;
                }

				if (city.MainBuilding.x == position.x && city.MainBuilding.y == position.y)
                {
                    continue;
                }
				
				if (RoadPathFinder.isRoadByMapPosition(position)) {
					if (!RoadPathFinder.hasPath(position, new Position(city.MainBuilding.x, city.MainBuilding.y), city, mapPos)) {
						return false;
					}
				}
			}

			return true;
		}

        private function validateTileCallback(x: int, y: int): * {
            // Get the screen position of the main building then we'll add the current tile x and y to get the point of this tile on the screen
			var screenPosition: ScreenPosition = TileLocator.getScreenPosition(city.primaryPosition.x, city.primaryPosition.y);

			if (!validateTile(new ScreenPosition(screenPosition.x + x, screenPosition.y + y)))
				return false;

			return new ColorTransform(1.0, 1.0, 1.0, 1.0, 100);
		}

		public function validateBuilding():void
		{
			var msg: XML;

			var city: City = Global.map.cities.get(parentObj.groupId);
			var mapObjPos: Position = objPosition.toPosition();

			// Check if cursor is inside city walls
			if (city != null && TileLocator.distance(city.primaryPosition.x, city.primaryPosition.y, 1, mapObjPos.x, mapObjPos.y, 1) >= city.radius)
            {
				hideCursors();
            }
			// Perform other validations
			else if (!validateTile(objPosition))
            {
				hideCursors();
            }
			else {
				showCursors();
            }
		}
	}
}


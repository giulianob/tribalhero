
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
    import src.Util.BinaryList.BinaryList;

    public class DestroyRoadCursor extends MovieClip implements IDisposable
	{
		private var objPosition: ScreenPosition = new ScreenPosition();

		private var city: City;

		private var originPoint: Point;

        private var destroyableRoads: BinaryList = new BinaryList(Position.sort, Position.compare);

        private var cursor: SimpleObject;
		private var destroyableArea: GroundCallbackCircle;
		private var parentObj: SimpleGameObject;

        // Since the client gets 1 tile update at a time, when the tiles change we dont want to refresh a lot of times
        // this timer is used to rate-limit the updating a bit
		private var redrawLaterTimer: Timer = new Timer(250);

		public function DestroyRoadCursor() { }

		public function init(parentObject: SimpleGameObject):void
		{
			doubleClickEnabled = true;

			this.parentObj = parentObject;

			city = Global.map.cities.get(parentObj.groupId);

			Global.gameContainer.setOverlaySprite(this);
			Global.map.selectObject(null);

            validateAllTiles();

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
                validateAllTiles();
				destroyableArea.redraw();
                validateBuilding();
			});

			Global.gameContainer.message.showMessage("Double click on a highlighted road to destroy it. Roads that are not highlighted may not be destroyed.");
		}

        private function validateAllTiles(): void {
            destroyableRoads.clear();

            var size: int = city.radius - 1;
            for each (var position: Position in TileLocator.foreachTile(city.primaryPosition.x, city.primaryPosition.y, size)) {
                validateTile(position);
            }
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

		private function validateTile(position: Position) : void {
			var tileType: int = Global.map.regions.getTileAt(position);

            // Make sure this tile is indeed a road
			if (!RoadPathFinder.isRoad(tileType)) {
				return;
            }

            // Make sure there is no structure at this point
			if (city.hasStructureAt(position)) {
				return;
            }

            // Make sure all structures have a diff path
			for each(var cityObject: CityObject in city.structures()) {
				if (cityObject.isMainBuilding) {
					continue;
                }

				if (ObjectFactory.isType("NoRoadRequired", cityObject.type)) {
					continue;
                }

				if (!RoadPathFinder.hasPath(cityObject.primaryPosition, cityObject.size, city, [ position ])) {
					return;
				}
			}

			// Make sure all neighbors have a different path
			for each (var neighborPosition: Position in TileLocator.foreachRadius(position.x, position.y, 1, false))
			{
                if (!RoadPathFinder.isRoadByMapPosition(neighborPosition)) {
                    continue;
                }

				if (city.hasStructureAt(neighborPosition))
                {
                    continue;
                }

                if (!RoadPathFinder.hasPath(neighborPosition, 1, city, [ position ])) {
                    return;
				}
			}

            destroyableRoads.add(position);
		}

        private function validateTileCallback(x: int, y: int): * {
            // Get the screen position of the main building then we'll add the current tile x and y to get the point of this tile on the screen
			var screenPosition: ScreenPosition = TileLocator.getScreenPosition(city.primaryPosition.x, city.primaryPosition.y);

			if (!destroyableRoads.get(new ScreenPosition(screenPosition.x + x, screenPosition.y + y).toPosition())) {
				return false;
            }

			return new ColorTransform(1.0, 1.0, 1.0, 1.0, 100);
		}

		public function validateBuilding():void
		{
			var city: City = Global.map.cities.get(parentObj.groupId);
			var mapObjPos: Position = objPosition.toPosition();

			// Check if cursor is inside city walls
			if (city != null && TileLocator.distance(city.primaryPosition.x, city.primaryPosition.y, 1, mapObjPos.x, mapObjPos.y, 1) >= city.radius)
            {
				hideCursors();
            }
			// Perform other validations
			else if (!destroyableRoads.get(mapObjPos))
            {
				hideCursors();
            }
			else {
				showCursors();
            }
		}
	}
}


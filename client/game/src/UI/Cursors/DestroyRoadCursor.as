
package src.UI.Cursors {
    import flash.events.TimerEvent;
    import flash.geom.Point;
    import flash.utils.Timer;

    import src.FeathersUI.Map.MapOverlayBase;

    import src.Global;
    import src.Map.*;
    import src.Objects.*;
    import src.Objects.Factories.*;
    import src.UI.Components.*;
    import src.UI.Sidebars.CursorCancel.CursorCancelSidebar;
    import src.Util.BinaryList.BinaryList;

    import starling.events.Touch;

    import starling.events.TouchEvent;
    import starling.events.TouchPhase;

    public class DestroyRoadCursor extends MapOverlayBase
	{
		private var objPosition: ScreenPosition = new ScreenPosition();

		private var city: City;

        private var destroyableRoads: BinaryList = new BinaryList(Position.sort, Position.compare);

        private var cursor: GroundCircle;
		private var destroyableArea: GroundCallbackCircle;
		private var parentObj: SimpleGameObject;

        // Since the client gets 1 tile update at a time, when the tiles change we dont want to refresh a lot of times
        // this timer is used to rate-limit the updating a bit
		private var redrawLaterTimer: Timer = new Timer(250);

		public function DestroyRoadCursor() { }

		public function init(parentObject: SimpleGameObject):void
		{
			this.parentObj = parentObject;

			city = Global.map.cities.get(parentObj.groupId);

			Global.gameContainer.setOverlaySprite(this);
			Global.map.selectObject(null);

            validateAllTiles();

			cursor = new GroundCircle(0, new ScreenPosition(), GroundCircle.GREEN);

			destroyableArea = new GroundCallbackCircle(city.radius - 1, city.primaryPosition.toScreenPosition(), validateTileCallback);
            destroyableArea.draw();

			var sidebar: CursorCancelSidebar = new CursorCancelSidebar(parentObj);
			Global.gameContainer.setSidebar(sidebar);

            addEventListener(TouchEvent.TOUCH, onTouched);

			Global.map.regions.addEventListener(RegionManager.REGION_UPDATED, update);

			redrawLaterTimer.addEventListener(TimerEvent.TIMER, function(e: *) : void {
				redrawLaterTimer.stop();
                validateAllTiles();
				destroyableArea.draw();
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

		private function update(e: * = null) : void {
			redrawLaterTimer.stop();
			redrawLaterTimer.start();
		}

		override public function dispose():void
		{
            super.dispose();

			Global.map.regions.removeEventListener(RegionManager.REGION_UPDATED, update);

			Global.gameContainer.message.hide();

			if (cursor != null)
			{
                cursor.dispose();
                destroyableArea.dispose();
			}

            removeEventListener(TouchEvent.TOUCH, onTouched);
		}

		private function showCursors() : void {
            cursor.draw();
		}

		private function hideCursors() : void {
            cursor.clear();
		}

		public function onTouched(event: TouchEvent):void
		{
            var clickTouch: Touch = event.getTouch(this, TouchPhase.BEGAN);
            if (clickTouch && clickTouch.tapCount == 2) {
                onDoubleClicked();
            }

            var moveTouch: Touch = event.getTouch(this, TouchPhase.HOVER);
            if (moveTouch) {
                var mousePos: Point = TileLocator.getPointWithZoomFactor(moveTouch.globalX, moveTouch.globalY);
                moveTo(mousePos.x, mousePos.y);
            }

            event.stopImmediatePropagation();
		}

		public function onDoubleClicked():void
		{
			if (!cursor.visible) {
				return;
            }

			var mapPos: Position = objPosition.toPosition();
			Global.mapComm.Region.destroyRoad(parentObj.groupId, mapPos.x, mapPos.y);
		}

		public function moveTo(globalX: int, globalY: int) : void
		{
			var pos: ScreenPosition = TileLocator.getActualCoord(
                    Global.gameContainer.camera.currentPosition.x + globalX,
                    Global.gameContainer.camera.currentPosition.y + globalY);

			if (!pos.equals(objPosition))
			{
				objPosition = pos;

                cursor.moveTo(pos);

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
			if (!destroyableRoads.get(new ScreenPosition(x, y).toPosition())) {
				return false;
            }

			return 0x640000;
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


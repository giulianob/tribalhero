
package src.UI.Cursors {
    import starling.events.*;
    import flash.geom.*;

    import src.*;
    import src.Map.*;
    import src.Objects.*;
    import src.Objects.Factories.*;
    import src.UI.Components.*;
    import src.UI.Sidebars.CursorCancel.*;
    import src.Util.BinaryList.BinaryList;

    public class BuildRoadCursor extends MapOverlayBase
	{
		private var objPosition: ScreenPosition = new ScreenPosition();

		private var city: City;

        private var buildableTiles: BinaryList = new BinaryList(Position.sort, Position.compare);

		private var cursor: GroundCircle;
		private var buildableArea: GroundCallbackCircle;
		private var parentObj: SimpleGameObject;

		public function BuildRoadCursor() { }

		public function init(parentObject: SimpleGameObject):void
		{
			this.parentObj = parentObject;

			city = Global.map.cities.get(parentObj.groupId);

			Global.gameContainer.setOverlaySprite(this);
			Global.map.selectObject(null);

            validateAllTiles();

			cursor = new GroundCircle(0, new ScreenPosition(), GroundCircle.SELECTED_GREEN);

			buildableArea = new GroundCallbackCircle(city.radius - 1, city.primaryPosition.toScreenPosition(), validateTileCallback);
            buildableArea.draw();

			var sidebar: CursorCancelSidebar = new CursorCancelSidebar(parentObj);
			Global.gameContainer.setSidebar(sidebar);

            addEventListener(TouchEvent.TOUCH, onTouched);

			Global.map.regions.addEventListener(RegionManager.REGION_UPDATED, update);

			Global.gameContainer.message.showMessage("Double click on the green squares to build roads.");
		}

        private function validateAllTiles(): void {
            buildableTiles.clear();

            var size: int = city.radius - 1;
            for each (var position: Position in TileLocator.foreachTile(city.primaryPosition.x, city.primaryPosition.y, size)) {
                validateTile(position);
            }
        }

		public function update(e: * = null) : void {
            validateAllTiles();
			buildableArea.draw();
			validateBuilding();
		}

		override public function dispose():void
		{
            super.dispose();

			Global.map.regions.removeEventListener(RegionManager.REGION_UPDATED, update);

			Global.gameContainer.message.hide();

            cursor.dispose();
            buildableArea.dispose();

            removeEventListener(TouchEvent.TOUCH, onTouched);
		}

		private function showCursors() : void {
			cursor.draw();
		}

		private function hideCursors() : void {
			cursor.clear();
		}

        public function onTouched(event: TouchEvent): void {
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
			Global.mapComm.Region.buildRoad(parentObj.groupId, mapPos.x, mapPos.y);
		}

		public function moveTo(x: int, y: int) : void
		{
			var pos: ScreenPosition = TileLocator.getActualCoord(Global.gameContainer.camera.currentPosition.x + x, Global.gameContainer.camera.currentPosition.y + y);

			if (!pos.equals(objPosition))
			{
				objPosition = pos;
				cursor.moveTo(objPosition);
				validateBuilding();
			}
		}

		private function validateTile(position: Position) : void {
			var tileType: int = Global.map.regions.getTileAt(position);

			if (RoadPathFinder.isRoad(tileType)) return;

			if (ObjectFactory.isType("TileResource", tileType)) return;

			if (city.hasStructureAt(position)) return;

			// Make sure there is a road next to this tile
			for each (var neighborPosition: Position in TileLocator.foreachRadius(position.x, position.y, 1, false))
			{
                var structure: CityObject = city.getStructureAt(neighborPosition);
				if (RoadPathFinder.isRoadByMapPosition(neighborPosition) && (structure == null || structure.isMainBuilding))
				{
                    buildableTiles.add(position);
					return;
                }
            }
		}

        private function validateTileCallback(x: int, y: int): * {
			if (!buildableTiles.get(new ScreenPosition(x, y).toPosition())) {
                return false;
            }

			return 0x7D9F43;
		}

		public function validateBuilding():void
		{
            var city: City = Global.map.cities.get(parentObj.groupId);
			var mapObjPos: Position = objPosition.toPosition();

			// Check if cursor is inside city walls
			if (city != null && TileLocator.distance(city.primaryPosition.x, city.primaryPosition.y, 1, mapObjPos.x, mapObjPos.y, 1) >= city.radius) {
				hideCursors();
			}
			// Perform other validations
			else if (!buildableTiles.get(mapObjPos)) {
				hideCursors();
			}
			else {
				showCursors();
			}
		}
	}
}



package src.UI.Cursors {
    import flash.display.*;
    import flash.events.*;
    import flash.geom.*;

    import src.*;
    import src.Map.*;
    import src.Objects.*;
    import src.Objects.Factories.*;
    import src.UI.Components.*;
    import src.UI.Sidebars.CursorCancel.*;
    import src.Util.BinaryList.BinaryList;

    public class BuildRoadCursor extends MovieClip implements IDisposable
	{
		private var objPosition: ScreenPosition = new ScreenPosition();

		private var city: City;

		private var originPoint: Point;

        private var buildableTiles: BinaryList = new BinaryList(Position.sort, Position.compare);

		private var cursor: SimpleObject;
		private var buildableArea: GroundCallbackCircle;
		private var parentObj: SimpleGameObject;

		public function BuildRoadCursor() { }

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

			buildableArea = new GroundCallbackCircle(city.radius - 1, validateTileCallback);
			buildableArea.alpha = 0.3;
			var pos: ScreenPosition = city.primaryPosition.toScreenPosition();
            buildableArea.x = buildableArea.primaryPosition.x = pos.x;
            buildableArea.y = buildableArea.primaryPosition.y = pos.y;
			
			Global.map.objContainer.addObject(buildableArea, ObjectContainer.LOWER);

			var sidebar: CursorCancelSidebar = new CursorCancelSidebar(parentObj);
			Global.gameContainer.setSidebar(sidebar);

			addEventListener(MouseEvent.DOUBLE_CLICK, onMouseDoubleClick);
			addEventListener(MouseEvent.CLICK, onMouseStop, true);
			addEventListener(MouseEvent.MOUSE_MOVE, onMouseMove);
			addEventListener(MouseEvent.MOUSE_OVER, onMouseStop);
			addEventListener(MouseEvent.MOUSE_DOWN, onMouseDown);

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

		public function update(e: Event = null) : void {
            validateAllTiles();
			buildableArea.redraw();
			validateBuilding();
		}

		public function dispose():void
		{
			Global.map.regions.removeEventListener(RegionManager.REGION_UPDATED, update);

			Global.gameContainer.message.hide();

			if (cursor != null)
			{
				if (cursor.stage != null) 
					Global.map.objContainer.removeObject(cursor, ObjectContainer.LOWER);
					
				if (buildableArea.stage != null) 
					Global.map.objContainer.removeObject(buildableArea, ObjectContainer.LOWER);
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
			if (!cursor.visible) return;
			if (Point.distance(TileLocator.getPointWithZoomFactor(event.stageX, event.stageY), originPoint) > city.radius) return;

			event.stopImmediatePropagation();

			var mapPos: Position = objPosition.toPosition();
			Global.mapComm.Region.buildRoad(parentObj.groupId, mapPos.x, mapPos.y);
		}

		public function onMouseDown(event: MouseEvent):void
		{
			originPoint = TileLocator.getPointWithZoomFactor(event.stageX, event.stageY);
		}

		public function onMouseMove(event: MouseEvent) : void
		{
			if (event.buttonDown) return;
			
			var mousePos: Point = TileLocator.getPointWithZoomFactor(Math.max(0, stage.mouseX), Math.max(0, stage.mouseY));
			var pos: ScreenPosition = TileLocator.getActualCoord(Global.gameContainer.camera.currentPosition.x + mousePos.x, Global.gameContainer.camera.currentPosition.y + mousePos.y);

			if (!pos.equals(objPosition))
			{
				objPosition = pos;

				//Object cursor
				if (cursor.stage != null) 
					Global.map.objContainer.removeObject(cursor, ObjectContainer.LOWER);

                cursor.x = cursor.primaryPosition.x = pos.x;
                cursor.y = cursor.primaryPosition.y = pos.y;
				
				Global.map.objContainer.addObject(cursor, ObjectContainer.LOWER);
				
				validateBuilding();
			}
		}

		private function validateTile(position: Position) : void {
			var tileType: int = Global.map.regions.getTileAt(position);

			if (RoadPathFinder.isRoad(tileType)) return;

			if (!ObjectFactory.isType("TileBuildable", tileType)) return;

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
            // Get the screen position of the city center then we'll add the current tile x and y to get the point of this tile on the screen
			var point: ScreenPosition = city.primaryPosition.toScreenPosition();

			if (!buildableTiles.get(new ScreenPosition(point.x + x, point.y + y).toPosition())) {
                return false;
            }

			return new ColorTransform(1.0, 1.0, 1.0, 1.0, 0, 100);
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


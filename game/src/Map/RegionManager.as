package src.Map
{
    import com.greensock.TweenMax;

    import flash.events.Event;
    import flash.geom.Point;

    import src.Global;
    import src.Objects.SimpleGameObject;
    import src.Objects.SimpleGameObject;
    import src.Util.BinaryList.*;
    import src.Util.Util;

    public class RegionManager extends BinaryList
	{
		public static const REGION_UPDATED: String = "REGION_UPDATED";

		public function RegionManager()
		{
			super(Region.sortOnId, Region.compare);
		}

        private function removeFromPrimaryRegionAndTiles(obj: SimpleGameObject, dispose: Boolean): Boolean
        {
            var regionId: int = TileLocator.getRegionId(obj.primaryPosition);
            var primaryRegion: Region = get(regionId);

            if (primaryRegion == null)
            {
                return false;
            }

            primaryRegion.removeObject(obj, dispose);

            for each (var position: Position in TileLocator.foreachMultitileObject(obj))
            {
                regionId = TileLocator.getRegionId(obj.primaryPosition);
                var region: Region = get(regionId);

                if (region == null) {
                    continue;
                }

                region.removeObjectFromTile(obj, position);
            }

            return true;
        }

        public function addObject(obj: SimpleGameObject, fadeIn: Boolean = true): void
        {
            var regionId: int = TileLocator.getRegionIdFromMapCoord(obj.primaryPosition.toPosition());
            var primaryRegion: Region = get(regionId);

            if (primaryRegion == null)
            {
                return;
            }

            var added: Boolean = primaryRegion.addObject(obj);

            if (!added) {
                return;
            }

            for each (var position: Position in TileLocator.foreachMultitileObject(obj))
            {
                regionId = TileLocator.getRegionIdFromMapCoord(position);
                var region: Region = get(regionId);

                if (region == null)
                {
                    continue;
                }

                region.addObjectToTile(obj, position);
            }

            if (fadeIn)
            {
                obj.fadeIn();
            }
        }

		public function updateObject(regionId: int, newObj: SimpleGameObject): SimpleGameObject
		{
			var region: Region = get(regionId);

			if (region == null) {
				return null;
            }

			var obj: SimpleGameObject = region.getObject(newObj.groupId, newObj.objectId);

			if (obj == null) {
				return null;
            }
			
			var objChanged: Boolean = !newObj.equalsOnMap(obj);
			
			var prevPosition: Point = new Point(obj.x, obj.y);

            removeFromPrimaryRegionAndTiles(obj, false);
			
			obj.copy(newObj);
			
			addObject(obj, objChanged);
			
			TweenMax.from(obj, 0.75, { x: prevPosition.x, y: prevPosition.y });
			
			Global.map.requeryIfSelected(obj);
			
			obj.dispatchEvent(new Event(SimpleGameObject.OBJECT_UPDATE));		
			
			return obj;
		}

		public function removeObject(regionId: int, groupId: int, objectId: int):void
		{
			var region: Region = get(regionId);

			if (region == null) {
				return;
            }

            var obj: SimpleGameObject = region.getObject(groupId, objectId);

            removeFromPrimaryRegionAndTiles(obj, true);
		}

		public function moveObject(oldRegionId: int, newObj: SimpleGameObject): SimpleGameObject
		{

            var oldRegion: Region = get(oldRegionId);

			if (oldRegion == null) {
                return null;
            }

            var currentObj: SimpleGameObject = oldRegion.getObject(newObj.groupId, newObj.objectId);
            removeFromPrimaryRegionAndTiles(currentObj, false);

			if (currentObj == null) {
                return null;
			}
			
			var objChanged: Boolean = !newObj.equalsOnMap(currentObj);
			
			var prevPosition: Point = new Point(currentObj.x, currentObj.y);

            currentObj.copy(newObj);
			addObject(currentObj, objChanged);
			
			TweenMax.from(currentObj, 0.75, { x: prevPosition.x, y: prevPosition.y });
				
			Global.map.requeryIfSelected(currentObj);

            currentObj.dispatchEvent(new Event(SimpleGameObject.OBJECT_UPDATE));
					
			return currentObj;
		}

		public function getObjectsInTile(position: Position, objClass: * = null): Array
		{
			var regionId: int = TileLocator.getRegionIdFromMapCoord(position);
			var region: Region = get(regionId);

			if (region == null) {
				return null;
            }

			return region.getObjectsInTile(position, objClass);
		}

		public function getTileAt(position: Position): int
		{
			var regionId: int = TileLocator.getRegionIdFromMapCoord(position);
			var region: Region = get(regionId);

			if (region == null)
				return -1;			

			return region.getTileAt(position);
		}

		public function setTileType(position: Position, tileType: int, redraw: Boolean = false) : void {
			var regionId: int = TileLocator.getRegionIdFromMapCoord(position);
			var region: Region = get(regionId);
			
			if (region == null)
				return;			
			
			region.setTile(position, tileType, redraw);
			
			dispatchEvent(new Event(REGION_UPDATED));
		}

		public function redrawRegion(regionId: int) : void {
			var region: Region = get(regionId);

			if (region == null)
				return;

			region.redraw();
		}
	}

}


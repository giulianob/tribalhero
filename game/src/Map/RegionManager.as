package src.Map
{
    import com.greensock.TweenMax;

    import flash.events.Event;
    import flash.geom.Point;

    import src.Global;
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

        private function addToPrimaryRegionAndTiles(obj: SimpleGameObject): Boolean
        {
            var regionId: int = TileLocator.getRegionId(obj.primaryPosition);
            var primaryRegion: Region = get(regionId);

            if (primaryRegion == null)
            {
                return false;
            }

            primaryRegion.addObject(obj);

            for each (var position: Position in TileLocator.foreachMultitileObject(obj))
            {
                regionId = TileLocator.getRegionId(obj.primaryPosition);
                var region: Region = get(regionId);

                if (region != null)
                {
                    region.addObjectToTile(obj, position);
                }
            }

            return true;
        }

		public function updateObject(regionId: int, newObj: SimpleGameObject): SimpleGameObject
		{
			var region: Region = get(regionId);

			if (region == null)
				return null;

			var obj: SimpleGameObject = region.getObject(newObj.groupId, newObj.objectId);

			if (obj == null) {
				return null;
            }
			
			var objChanged: Boolean = !newObj.equalsOnMap(obj);
			
			var prevPosition: Point = new Point(obj.x, obj.y);
						
			region.removeObject(obj.groupId, obj.objectId, false);
			
			obj.copy(newObj);
			
			addObject(regionId, obj, objChanged);
			
			TweenMax.from(obj, 0.75, { x: prevPosition.x, y: prevPosition.y });
			
			Global.map.requeryIfSelected(obj);
			
			obj.dispatchEvent(new Event(SimpleGameObject.OBJECT_UPDATE));		
			
			return obj;
		}

		public function removeObject(regionId: int, groupId: int, objId: int):void
		{
			var region: Region = get(regionId);

			if (region == null) {
				return;
            }
			
			region.removeObject(groupId, objId);
		}

		public function moveObject(oldRegionId: int, newRegionId: int, newObj: SimpleGameObject): SimpleGameObject
		{
			var oldRegion: Region = get(oldRegionId);

			if (oldRegion == null)
				return null;
			
			var obj: SimpleGameObject = oldRegion.removeObject(newObj.groupId, newObj.objectId, false);

			if (obj == null) {
				return null; 
			}
			
			var objChanged: Boolean = !newObj.equalsOnMap(obj);
			
			var prevPosition: Point = new Point(obj.x, obj.y);
			
			obj.copy(newObj);
			addObject(newRegionId, obj, objChanged);
			
			TweenMax.from(obj, 0.75, { x: prevPosition.x, y: prevPosition.y });
				
			Global.map.requeryIfSelected(obj);
			
			obj.dispatchEvent(new Event(SimpleGameObject.OBJECT_UPDATE));
					
			return obj;
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

		public function getTileAt(x: int, y: int): int
		{
			var regionId: int = TileLocator.getRegionIdFromMapCoord(new Position(x, y));
			var region: Region = get(regionId);

			if (region == null)
				return -1;			

			return region.getTileAt(x, y);
		}

		public function setTileType(x: int, y: int, tileType: int, redraw: Boolean = false) : void {
			var regionId: int = TileLocator.getRegionIdFromMapCoord(new Position(x, y));
			var region: Region = get(regionId);
			
			if (region == null)
				return;			
			
			region.setTile(x, y, tileType, redraw);
			
			dispatchEvent(new Event(REGION_UPDATED));
		}

		public function redrawRegion(regionId: int) : void {
			var region: Region = get(regionId);

			if (region == null)
				return;

			region.redraw();
		}

		public function addObject(regionId: int, obj: SimpleGameObject, fadeIn: Boolean = true): SimpleGameObject
		{
			var region: Region = get(regionId);

			if (region == null)
			{
				Util.log("Invalid region at map.addObject. Region received: " + regionId);
				return null;
			}

			var obj:SimpleGameObject = region.addObject(obj);			
			
			if (obj && fadeIn)
            {
				obj.fadeIn();
			}
			
			return obj;
		}
	}

}


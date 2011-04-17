package src.Map
{
	import flash.events.Event;
	import flash.geom.Point;
	import src.Global;
	import src.Objects.GameObject;
	import src.Util.Util;
	import src.Objects.SimpleGameObject;
	import src.Util.BinaryList.*;

	/**
	 * ...
	 * @author Giuliano Barberi
	 */
	public class RegionList extends BinaryList
	{
		public static const REGION_UPDATED: String = "REGION_UPDATED";

		public function RegionList()
		{
			super(Region.sortOnId, Region.compare);
		}

		public function updateObject(regionId: int, newObj: SimpleGameObject): SimpleGameObject
		{
			var region: Region = get(regionId);

			if (region == null)
				return null;

			var obj: SimpleGameObject = region.getObject(newObj.groupId, newObj.objectId);

			if (obj == null)
				return null;

			var reselect: Boolean = Global.map.selectedObject != null && Global.map.selectedObject is SimpleGameObject && newObj.equalById(Global.map.selectedObject);

			newObj.copy(obj);
			region.removeObject(obj.groupId, obj.objectId);
			obj = addObject(regionId, newObj);
			if (!newObj.equalsOnMap(obj))
				obj.fadeIn();			
			obj = newObj;
			
			if (reselect) 
				Global.map.selectObject(obj);
			
			obj.dispatchEvent(new Event(SimpleGameObject.OBJECT_UPDATE));
			
			return obj;			
		}

		public function removeObjectByObj(obj: SimpleGameObject):void {
			removeObject(MapUtil.getRegionId(obj.getX(), obj.getY()), obj.groupId, obj.objectId);
		}

		public function removeObject(regionId: int, groupId: int, objId: int):void
		{
			var region: Region = get(regionId);

			if (region == null)
				return;

			var obj: SimpleGameObject = region.removeObject(groupId, objId);

			if (Global.map.selectedObject == obj)
				Global.map.selectObject(null);
		}

		public function moveObject(oldRegionId: int, newRegionId: int, newObj: SimpleGameObject): SimpleGameObject
		{
			var oldRegion: Region = get(oldRegionId);

			if (oldRegion == null)
				return null;

			var gameObject: SimpleGameObject = oldRegion.removeObject(newObj.groupId, newObj.objectId, false);

			if (gameObject == null)
				return null;

			var newRegion:Region = get(newRegionId);

			if (newRegion == null)
				return gameObject;

			newRegion.addObject(gameObject);

			return updateObject(newRegionId, newObj);
		}

		public function getObjectsAt(x: int, y: int, objClass: Class = null): Array
		{
			var regionId: int = MapUtil.getRegionId(x, y);
			var region: Region = get(regionId);

			if (region == null)
				return null;

			return region.getObjectsAt(x, y, objClass);
		}

		public function getTileAt(x: int, y: int): int
		{
			var regionId: int = MapUtil.getRegionIdFromMapCoord(x, y);
			var region: Region = get(regionId);

			if (region == null)
				return -1;			

			return region.getTileAt(x, y);
		}

		public function setTileType(x: int, y: int, tileType: int, redraw: Boolean = false) : void {
			var regionId: int = MapUtil.getRegionIdFromMapCoord(x, y);
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

		public function addObject(regionId: int, obj: SimpleGameObject): SimpleGameObject
		{
			var region: Region = get(regionId);

			if (region == null)
			{
				Util.log("Invalid region at map.addObject. Region received: " + regionId);
				return null;
			}

			var obj:SimpleGameObject = region.addObject(obj);

			if (!obj)
				return null;

			obj.moveWithCamera(Global.gameContainer.camera);

			return obj;
		}
	}

}


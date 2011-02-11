package src.Map
{
	import flash.events.Event;
	import flash.geom.Point;
	import src.Global;
	import src.Objects.GameObject;
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

		public function updateObject(regionId: int, playerId: int, cityId: int, objectId: int, type:int, level: int, hpPercent: int, objX: int, objY : int): SimpleGameObject
		{
			var region: Region = get(regionId);

			if (region == null)
			return null;

			var obj: SimpleGameObject = region.getObject(cityId, objectId);

			if (obj == null)
			return null;

			var reselect: Boolean = Global.map.selectedObject == obj;

			if (level != obj.level || type != obj.type)
			{
				region.removeObject(cityId, objectId);

				var wallRadius: int = obj.wall.radius; //preserve wall

				obj = addObject(obj, regionId, level, type, playerId, cityId, objectId, hpPercent, objX, objY);

				obj.fadeIn();				

				if (wallRadius > 0)
				obj.wall.draw(wallRadius);
			}
			else
			{
				var coord: Point = MapUtil.getScreenCoord(objX, objY);

				Global.map.objContainer.removeObject(obj, 0, false);

				obj.setProperties(level, hpPercent, coord.x, coord.y);

				Global.map.objContainer.addObject(obj);

				obj.moveWithCamera(Global.gameContainer.camera);
			}
			
			if (reselect && obj is GameObject) Global.map.selectObject(obj as GameObject);			

			return obj;
		}

		public function removeObjectByObj(obj: SimpleGameObject):void {
			removeObject(MapUtil.getRegionId(obj.getX(), obj.getY()), obj.cityId, obj.objectId);
		}

		public function removeObject(regionId: int, cityId: int, objId: int):void
		{
			var region: Region = get(regionId);

			if (region == null)
			return;

			region.removeObject(cityId, objId);

			if (Global.map.selectedObject && Global.map.selectedObject.cityId == cityId && Global.map.selectedObject.objectId == objId)
			Global.map.selectObject(null);
		}

		public function moveObject(oldRegionId: int, newRegionId: int, level: int, type: int, playerId: int, cityId: int, objectId: int, hpPercent: int, objX: int, objY : int): SimpleGameObject
		{
			var oldRegion: Region = get(oldRegionId);

			if (oldRegion == null)
			return null;

			var gameObject: SimpleGameObject = oldRegion.removeObject(cityId, objectId, false);

			if (gameObject == null)
			return null;

			var newRegion:Region = get(newRegionId);

			if (newRegion == null)
			return gameObject;

			newRegion.addGameObject(gameObject);

			return updateObject(newRegionId, playerId, cityId, objectId, type, level, hpPercent, objX, objY);
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

			if (region == null) {
				return -1;
			}

			return region.getTileAt(x, y);
		}

		public function setTileType(x: int, y: int, tileType: int, redraw: Boolean = false) : void {
			var regionId: int = MapUtil.getRegionIdFromMapCoord(x, y);
			var region: Region = get(regionId);

			if (region == null) {
				return;
			}

			region.setTile(x, y, tileType, redraw);
			if (!redraw) dispatchEvent(new Event(REGION_UPDATED));
		}

		public function redrawRegion(regionId: int) : void {
			var region: Region = get(regionId);

			if (region == null) {
				return;
			}

			region.redraw();
		}

		public function addObject(baseObj: SimpleGameObject, regionId: int, level: int, type: int, playerId: int, cityId: int, objectId: int, hpPercent: int, objX: int, objY : int): SimpleGameObject
		{
			var region: Region = get(regionId);

			if (region == null)
			{
				trace("Invalid region at map.addObject. Region received: " + regionId);
				return null;
			}

			var obj:SimpleGameObject = region.addObject(level, type, playerId, cityId, objectId, hpPercent, objX, objY);

			if (!obj)
			return null;

			if (baseObj != null)
			obj.copy(baseObj);

			obj.moveWithCamera(Global.gameContainer.camera);

			return obj;
		}
	}

}


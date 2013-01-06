package src.Map
{
	import com.greensock.easing.Elastic;
	import com.greensock.easing.Linear;
	import com.greensock.TweenMax;
	import flash.events.Event;
	import flash.geom.Point;
	import src.Global;
	import src.Objects.GameObject;
	import src.Objects.SimpleObject;
	import src.Util.Util;
	import src.Objects.SimpleGameObject;
	import src.Util.BinaryList.*;

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

			if (region == null)
				return;
			
			var obj: SimpleGameObject = region.removeObject(groupId, objId);
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

		public function getObjectsAt(x: int, y: int, objClass: * = null): Array
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

		public function addObject(regionId: int, obj: SimpleGameObject, fadeIn: Boolean = true): SimpleGameObject
		{
			var region: Region = get(regionId);

			if (region == null)
			{
				Util.log("Invalid region at map.addObject. Region received: " + regionId);
				return null;
			}

			var obj:SimpleGameObject = region.addObject(obj);			
			
			if (obj && fadeIn) {
				obj.fadeIn();
			}
			
			return obj;
		}
	}

}


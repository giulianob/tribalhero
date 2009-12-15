package src.Map 
{
	import flash.events.Event;
	import flash.geom.Point;
	import src.Constants;
	import src.Objects.GameObject;
	import src.Objects.SimpleGameObject;
	import src.Util.BinaryList;
	
	/**
	* ...
	* @author Giuliano Barberi
	*/
	public class RegionList extends BinaryList
	{
		private var map: Map;
		
		public function RegionList(map: Map) 
		{
			super(Region.sortOnId, Region.compare);
			this.map = map;
		}
		
		public function updateObject(regionId: int, playerId: int, cityId: int, objectId: int, type:int, level: int, hpPercent: int, objX: int, objY : int): SimpleGameObject
		{
			var region: Region = get(regionId);
			
			if (region == null)
				return null;
			
			var obj: SimpleGameObject = region.getObject(cityId, objectId);
			
			if (obj == null)
				return null;
			
			if (level != obj.level || type != obj.type)
			{			
				var reselect: Boolean = map.selectedObject == obj;
				
				if (map.selectedObject == obj)
					map.selectObject(null);
				
				region.removeObject(cityId, objectId);
				
				var wallRadius: int = obj.wall.radius; //preserve wall
				
				obj = addObject(obj, regionId, level, type, playerId, cityId, objectId, hpPercent, objX, objY);							
				
				obj.fadeIn();
				
				if (reselect && obj is GameObject)
					map.selectObject(obj as GameObject);
				
				if (wallRadius > 0)
					obj.wall.draw(wallRadius);
			}
			else
			{
				var coord: Point = MapUtil.getScreenCoord(objX, objY);
				
				map.objContainer.removeObject(obj, 0, false);
				
				obj.setProperties(level, hpPercent, coord.x, coord.y);
				
				map.objContainer.addObject(obj);
				
				obj.moveWithCamera(map.gameContainer.camera);
			}
			
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
			
			if (map.selectedObject && map.selectedObject.cityId == cityId && map.selectedObject.objectId == objId)
				map.selectObject(null);
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
		
		public function getObjectAt(x: int, y: int): SimpleGameObject
		{			
			var regionId: int = MapUtil.getRegionId(x, y);
			var region: Region = get(regionId);
			
			if (region == null)
				return null;
			
			return region.objectAt(x, y);			
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
			
			obj.moveWithCamera(map.gameContainer.camera);
			
			return obj;
		}
	}
	
}
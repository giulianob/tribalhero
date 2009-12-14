/**
* ...
* @author Default
* @version 0.1
*/

package src.Map {
	import flash.events.Event;
	import flash.events.EventDispatcher;
	import flash.geom.Point;
	import src.Objects.Actions.*;
	import src.Objects.Battle.BattleManager;
	import src.Objects.Prototypes.EffectPrototype;
	import src.Objects.*;
	import src.Util.BinaryList;
	import src.Util.Util;
	
	public class City extends EventDispatcher
	{
		public static var RESOURCES_UPDATE: String = "RESOURCES_UPDATE";
		public static var RADIUS_UPDATE: String = "RADIUS_UPDATE";
		
		public var id: int;
		public var resources: LazyResources;
		public var name: String;
		public var radius: int = 0;
		
		public var battle: BattleManager;
		
		public var techManager: TechnologyManager = new TechnologyManager(EffectPrototype.LOCATION_CITY);
		
		public var currentActions: CurrentActionManager = new CurrentActionManager();
		public var notifications: NotificationManager = new NotificationManager();
		
		public var troops: TroopManager = new TroopManager();		
		public var objects: BinaryList = new BinaryList(CityObject.sortOnId, CityObject.compareObjId);
		public var template: TemplateManager = new TemplateManager();			
		
		public function get MainBuilding() : CityObject {
			return objects.get(1);
		}
		
		public function City(id: int, name: String, radius: int, resources: LazyResources) {
			this.id = id;
			this.resources = resources;
			this.radius = radius;
			this.name = name;
			
			dispatchEvent(new Event(RESOURCES_UPDATE));
		}
			
		public function nearObjects(mindist: int, maxdist: int, x: int, y: int, type: int = -1): Array
		{
			var ret: Array = new Array();
			
			var pos: Point = MapUtil.getMapCoord(x, y);
			
			for each(var obj: CityObject in objects.each())
			{
				if (type != -1 && obj.type != type)
					continue;

				var dist: Number = obj.distance(pos.x, pos.y);

				if (mindist <= dist && maxdist >= dist)
					ret.push(obj);
			}
			
			return ret;
		}
		
		public function getBusyLaborCount() : int {
			var labors: int = 0;
			
			for each(var obj: CityObject in objects.each())
			{
				labors += obj.labor;
			}
			
			return labors;
		}
		
		public static function sortOnId(a:City, b:City):Number 
		{
			var aId:Number = a.id;
			var bId:Number = b.id;

			if(aId > bId) {
				return 1;
			} else if(aId < bId) {
				return -1;
			} else  {
				return 0;
			}
		}
		
		public static function compare(a: City, value: int):int
		{
			return a.id - value;
		}						
	}
	
}

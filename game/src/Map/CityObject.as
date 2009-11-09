package src.Map {
	import flash.geom.Point;
	import src.Objects.Actions.CurrentAction;
	import src.Objects.IObject;
	import src.Objects.Prototypes.EffectPrototype;
	import src.Objects.TechnologyManager;
	import src.Util.BinaryList;
	
	/**
	* ...
	* @author Default
	*/
	public class CityObject implements IObject {
		
		public var type: int;
		public var level: int;
		public var x: int;
		public var y: int;
		public var objectId: int;
		public var city: City;
		
		public var techManager: TechnologyManager;
		
		public function CityObject(city: City, objectId: int, type: int, level: int, x: int, y: int) {
			this.city = city;
			this.objectId = objectId;
			this.type = type;
			this.level = level;
			this.x = x;
			this.y = y;			
			techManager = new TechnologyManager(EffectPrototype.LOCATION_OBJECT, city.techManager);
		}	
		
		public function getCityId(): int
		{
			return city.id;
		}
		
		public function getLevel(): int
		{
			return level;
		}
		
		public function getType(): int
		{
			return type;
		}
		
		public static function sortOnId(a:CityObject, b:CityObject):Number 
		{
			var aId:Number = a.objectId;
			var bId:Number = b.objectId;

			if(aId > bId) {
				return 1;
			} else if(aId < bId) {
				return -1;
			} else  {
				return 0;
			}
		}
		
		public static function sortOnCityIdAndObjId(a:CityObject, b:CityObject):Number {
			var aCityId:Number = a.city.id;
			var bCityId:Number = b.city.id;

			var aObjId:Number = a.objectId;
			var bObjId:Number = b.objectId;
			
			if (aCityId > bCityId)
				return 1;
			else if (aCityId < bCityId)
				return -1;
			else if (aObjId > bObjId)
				return 1;
			else if (aObjId < bObjId)
				return -1;
			else
				return 0;
		}
		
		public function distance(x_1: int, y_1: int): int
		{
            var offset: int = 0;
			
			var objPos: Point = new Point();
			
			objPos.x = x;
			objPos.y = y;
			
            if (objPos.y % 2 == 1 && y_1 % 2 == 0 && x_1 <= objPos.x) offset = 1;
            if (objPos.y % 2 == 0 && y_1 % 2 == 1 && x_1 >= objPos.x) offset = 1;
			
            return ((x_1 > objPos.x ? x_1 - objPos.x : objPos.x - x_1) + (y_1 > objPos.y ? y_1 - objPos.y : objPos.y - y_1) / 2 + offset);		
        }
		
		public static function compareCityIdAndObjId(a: CityObject, value: Array):int
		{
			var cityDelta: int = a.city.id - value[0];
			var idDelta: int = a.objectId - value[1];
			
			if (cityDelta != 0)
				return cityDelta;
				
			if (idDelta != 0)
				return idDelta;
			else
				return 0;
		}		
		
		public static function compareObjId(a: CityObject, value: int):int
		{
			return a.objectId - value;
		}	
	}
	
}
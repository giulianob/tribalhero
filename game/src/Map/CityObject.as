package src.Map {
	import src.Objects.*;
	import src.Objects.Factories.*;
	import src.Objects.Prototypes.*;
	import src.Objects.States.GameObjectState;
	import src.Util.BinaryList.*;

	public class CityObject {
		public var state:GameObjectState;

		public var type: int;
		public var level: int;
		public var x: int;
		public var y: int;
		public var objectId: int;
		public var city: City;
		public var labor: int;

		public var techManager: TechnologyManager;

		public function CityObject(city: City, objectId: int, type: int, level: int, state: GameObjectState, x: int, y: int, labor: int) {
			this.state = state;
			this.city = city;
			this.objectId = objectId;
			this.type = type;
			this.level = level;
			this.x = x;
			this.y = y;
			this.labor = labor;
			techManager = new TechnologyManager(EffectPrototype.LOCATION_OBJECT, city.techManager);
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
			return MapUtil.distance(x, y, x_1, y_1);
		}

		public function radiusDistance(x_1: int, y_1: int): int
		{
			return MapUtil.radiusDistance(x, y, x_1, y_1);
		}
		
		public function getStructurePrototype(): StructurePrototype 
		{
			return StructureFactory.getPrototype(type, level);
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


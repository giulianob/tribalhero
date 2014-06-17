package src.Map {
    import src.Objects.*;
    import src.Objects.Factories.*;
    import src.Objects.Prototypes.*;
    import src.Objects.States.GameObjectState;

    public class CityObject {
		public var state:GameObjectState;

		public var type: int;
		public var level: int;
		public var objectId: int;
		public var city: City;
		public var labor: int;
        public var size: int;
        public var primaryPosition: Position = new Position();
		public var techManager: TechnologyManager;
        public var theme: String;

		public function CityObject(city: City, objectId: int, theme: String, type: int, level: int, state: GameObjectState, x: int, y: int, size: int, labor: int) {
			this.state = state;
            this.theme = theme;
			this.city = city;
			this.objectId = objectId;
			this.type = type;
			this.level = level;
			this.x = x;
			this.y = y;
			this.labor = labor;
            this.size = size;

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

		public function getStructurePrototype(): StructurePrototype
		{
			return StructureFactory.getPrototype(type, level);
		}

        public function get isMainBuilding():Boolean
        {
            return objectId == 1;
        }

        public static function compareObjId(a: CityObject, value: int):int
		{
			return a.objectId - value;
		}

        public function get x():int
        {
            return primaryPosition.x;
        }

        public function set x(value:int):void
        {
            primaryPosition.x = value;
        }

        public function get y():int
        {
            return primaryPosition.y;
        }

        public function set y(value:int):void
        {
            primaryPosition.y = value;
        }
	}

}


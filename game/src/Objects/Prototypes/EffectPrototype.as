package src.Objects.Prototypes {
	
	/**
	* ...
	* @author Default
	*/
	public class EffectPrototype {
		
		public static const EFFECT_BUILD_TIME_MULTIPLIER: int = 0;
		public static const EFFECT_RESOURCE_RATE: int = 1;
		public static const EFFECT_BUILD_HEAVY_TANK: int = 2;
		public static const EFFECT_TRAIN_TIME_MULTIPLIER: int = 3;
		public static const EFFECT_BUILD_NINJA: int = 4;
		public static const EFFECT_CAN_BUILD: int = 5;
		public static const EFFECT_HARVEST_SURPRISE: int = 6;	
		public static const EFFECT_OVER_DIGGING: int = 7;
		public static const EFFECT_WOOD_CUTTING: int = 8;
		public static const EFFECT_HAVE_TECHNOLOGY: int = 9;
		public static const EFFECT_COUNT_LESS_THAN: int = 10;
		public static const EFFECT_REPAIR_SAVING: int = 11;
		public static const EFFECT_TROOP_SPEED_MOD: int = 12;
		public static const EFFECT_AWAY_FROM_STRUCTURE_MOD: int = 14;
		public static const EFFECT_TRADE_SPEED_MOD: int = 18;
        public static const EFFECT_LABOR_MOVE_TIME_MOD: int = 20;
		public static const EFFECT_LABOR_TRAIN_TIME_MOD: int = 22;
		public static const EFFECT_REDUCE_UPKEEP: int = 103;
		
		public static const LOCATION_OBJECT: int = 0;
		public static const LOCATION_CITY: int = 1;
		public static const LOCATION_PLAYER: int = 2;
		public static const LOCATION_TROOP: int = 3;
	
		public static const INHERIT_INVISIBLE: int = 1;
		public static const INHERIT_SELF: int = 2;
		public static const INHERIT_SELF_ALL: int = 3;
		public static const INHERIT_UPWARD: int = 4;
		public static const INHERIT_ALL: int = 7;		
		
		public var effectCode: int;
		public var isPrivate: Boolean;
		public var location: int;
		public var param1: *;
		public var param2: *;
		public var param3: *;
		public var param4: *;
		public var param5: *;		
		
		public function EffectPrototype() {

		}
		
		public static function sortOnType(a:EffectPrototype, b:EffectPrototype):Number {
			var aType:Number = a.effectCode;
			var bType:Number = b.effectCode;

			if(aType > bType) {
				return 1;
			} else if(aType < bType) {
				return -1;
			} else  {
				return 0;
			}
		}
		
		public static function compare(a: EffectPrototype, value: int):int
		{
			return a.effectCode - value;
		}
	}
	
}
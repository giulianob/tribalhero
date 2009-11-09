package src.Objects.Prototypes {
	
	/**
	* ...
	* @author Default
	*/
	public class EffectReqPrototype {
		
		public var method: String;
		public var param1: int;
		public var param2: int;
		public var param3: int;
		public var param4: int;
		public var param5: int;
		
		public function EffectReqPrototype() {
			
		}
		
		public static function sortOnMethod(a:EffectReqPrototype, b:EffectReqPrototype):Number {
			var aType:String = a.method;
			var bType:String = b.method;

			if(aType > bType) {
				return 1;
			} else if(aType < bType) {
				return -1;
			} else  {
				return 0;
			}
		}
		
		public static function compare(a: EffectReqPrototype, value: String):int
		{
			if (a.method < value)
				return -1;
			else if (a.method > value)
				return 1;
			else
				return 0;
		}
		
	}
	
}
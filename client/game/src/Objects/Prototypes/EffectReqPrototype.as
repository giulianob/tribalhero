package src.Objects.Prototypes {
	
	/**
	* ...
	* @author Default
	*/
	public class EffectReqPrototype {
		
		public var method: String;
		public var param1: String;
		public var param2: String;
		public var param3: String;
		public var param4: String;
		public var param5: String;
		public var description: String;
		
		public function EffectReqPrototype() {
			
		}
		
		public static function asMessage(message: String) : EffectReqPrototype {
			var req: EffectReqPrototype = new EffectReqPrototype();
			req.method = "Message";
			req.param1 = message;
			
			return req;
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
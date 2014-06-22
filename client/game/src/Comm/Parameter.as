package src.Comm {
	internal class Parameter {
		public var obj:*;
		public var srcType:int;
		public var destType:int;
		
		public static const INT1: int 	= 1;
		public static const UINT1: int 	= 2;
		public static const INT2: int 	= 3;
		public static const UINT2: int 	= 4;
		public static const INT4: int 	= 5;
		public static const UINT4: int 	= 6;
		public static const FLOAT: int 	= 7;
		public static const STRING: int 	= 8;		
	
		public function Parameter(value:*,src:int,dest:int) {
			obj = value;
			srcType = src;
			destType = dest;
		}

	}
}
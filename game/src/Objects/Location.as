package src.Objects 
{
	/**
	 * ...
	 * @author Anthony Lam
	 */
	public class Location 
	{
        public static const CITY: int = 1;
        public static const STRONGHOLD :int = 2;
		public static const BARBARIAN: int = 3;
		
		public var type :int;
		public var id :uint;
		public var objectId :uint;
		
		public function Location(type: int, id: uint, objectId: uint = 0) 
		{
			this.id = id;
			this.type = type;
			this.objectId = objectId;
		}
		
	}

}
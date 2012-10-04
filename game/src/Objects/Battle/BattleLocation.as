package src.Objects.Battle 
{
	public class BattleLocation 
	{		
        public static const CITY: int = 0;
        public static const STRONGHOLD: int = 1;
        public static const STRONGHOLDGATE: int = 2;
		
		public var name:String;
		public var id:int;
		public var type:int;
		
		public function BattleLocation(type: int, id: int, name: String = "")
		{
			this.name = name;
			this.id = id;
			this.type = type;
			
		}		
		
		public function getLocationTypeAsString(): String {
			switch (type) {
				case CITY:
					return "City";
				case STRONGHOLD:
					return "Stronghold";
				case "StrongholdGate":
					return "StrongholdGate";
				default:
					throw new Error("Unknown location type");
			}
		}
	}
}
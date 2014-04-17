package src.Objects.Battle 
{
	public class BattleLocation 
	{		
        public static const CITY: int = 0;
        public static const STRONGHOLD: int = 1;
        public static const STRONGHOLDGATE: int = 2;
        public static const BARBARIANTRIBE: int = 3;
		
		public var name:String;
		public var id:int;
		public var type:int;
		
		public function BattleLocation(type: *, id: int, name: String = "")
		{			
			this.name = name;
			this.id = id;
			
			if (type is int) {
				this.type = type;
			}
			else {
				switch (type) {
					case "City":
						this.type = CITY;
						break;
					case "Stronghold":
						this.type = STRONGHOLD;
						break;					
					case "StrongholdGate":
						this.type = STRONGHOLDGATE;
						break;		
					case "BarbarianTribe":
						this.type = BARBARIANTRIBE;
						break;		                        
					default:
						throw new Error("Unknown location type");						
				}
			}
			
		}		
		
		public function getLocationTypeAsString(): String {
			return BattleLocation.getLocationTypeAsString(type);
		}
		
		public static function getLocationTypeAsString(type: int): String {
			switch (type) {
				case CITY:
					return "City";
				case STRONGHOLD:
					return "Stronghold";
				case STRONGHOLDGATE:
					return "StrongholdGate";
                case BARBARIANTRIBE:
                    return "BarbarianTribe";                    
				default:
					throw new Error("Unknown location type");
			}
		}
	}
}
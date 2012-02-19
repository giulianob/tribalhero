package src.Objects.Battle {
	
	/**
	* ...
	* @author Default
	*/
	public class CombatObject {
		public var playerId: int;
		public var cityId: int;
		public var combatObjectId: int;
		public var type: int;
		public var level: int;
		public var hp: int;
		public var maxHp: int;
		public var troopStubId: int;
		
		public function CombatObject(playerId: int, cityId: int, combatObjectId: int, troopStubId: int, type: int, level: int, hp: int, maxHp: int)
		{
			this.playerId = playerId;
			this.cityId = cityId;
			this.combatObjectId = combatObjectId;
			this.type = type;
			this.troopStubId = troopStubId;
			this.level = level;
			this.hp = hp;
			this.maxHp = maxHp;
		}
		
		public function get name(): String {
			return "Unimplemented";
		}
		
		public static function sortOnId(a:CombatObject, b:CombatObject):Number 
		{
			var aId:Number = a.combatObjectId;
			var bId:Number = b.combatObjectId;

			if(aId > bId) {
				return 1;
			} else if(aId < bId) {
				return -1;
			} else  {
				return 0;
			}
		}
		
		public static function compareObjId(a: CombatObject, value: int):int
		{
			return a.combatObjectId - value;
		}	

	}
	
}
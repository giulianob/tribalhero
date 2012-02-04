package src.Objects.Battle {
	import src.Map.City;
	import src.Objects.Factories.UnitFactory;
	import src.Objects.Prototypes.UnitPrototype;
	
	/**
	* ...
	* @author Default
	*/
	public class CombatUnit extends CombatObject
	{		
		private var prototype: UnitPrototype;
		
		public function CombatUnit(playerId: int, cityId: int, combatObjectId: int, troopStubId: int, type: int, level: int, hp: int, maxHp: int)
		{
			super(playerId, cityId, combatObjectId, troopStubId, type, level, hp, maxHp);
			
			prototype = UnitFactory.getPrototype(type, level);
		}
		
		public override function get name(): String {
			if (!prototype)
				return "Unknown";
				
			return prototype.getName();
		}				
	}
	
}
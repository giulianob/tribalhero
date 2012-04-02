package src.Objects.Battle {	
	import src.Objects.Factories.StructureFactory;
	import src.Objects.Prototypes.StructurePrototype;
	/**
	* ...
	* @author Default
	*/
	public class CombatStructure extends CombatObject
	{		
		private var prototype: StructurePrototype;
		
		public function CombatStructure(playerId: int, cityId: int, combatObjectId: int, troopStubId: int, type: int, level: int, hp: int, maxHp: int)
		{
			super(playerId, cityId, combatObjectId, troopStubId, type, level, hp, maxHp);
						
			prototype = StructureFactory.getPrototype(type, level);
		}
		
		public override function get name(): String {
			if (!prototype)
				return "Unknown";
			
			return prototype.getName();
		}				
	}
	
}
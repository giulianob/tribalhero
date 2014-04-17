package src.Objects.Battle {
    import src.Objects.Factories.UnitFactory;
    import src.Objects.Prototypes.UnitPrototype;

    public class CombatUnit extends CombatObject
	{		
		private var prototype: UnitPrototype;
		
		public function CombatUnit(combatObjectId: int, type: int, level: int, hp: Number, maxHp: Number)
		{
			super(combatObjectId, type, level, hp, maxHp);
			
			prototype = UnitFactory.getPrototype(type, level);
		}
		
		public override function get name(): String {
			if (!prototype)
				return "Unknown";
				
			return prototype.getName();
		}				
	}
	
}
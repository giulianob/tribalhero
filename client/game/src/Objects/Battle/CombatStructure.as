package src.Objects.Battle {
    import flash.display.*;

    import src.Objects.Factories.*;
    import src.Objects.Prototypes.*;

    public class CombatStructure extends CombatObject
	{		
		private var prototype: StructurePrototype;
		
		public function CombatStructure(combatObjectId: int, type: int, level: int, hp: Number, maxHp: Number, count: int)
		{
			super(combatObjectId, type, level, hp, maxHp, count);
						
			prototype = StructureFactory.getPrototype(type, level);
		}

		public override function get name(): String {
			if (!prototype)
				return "Unknown";
			
			return prototype.getName();
		}				
	}
	
}
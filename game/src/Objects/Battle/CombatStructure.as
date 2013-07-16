package src.Objects.Battle {
    import flash.display.*;

    import src.Objects.Factories.*;
    import src.Objects.Prototypes.*;

    public class CombatStructure extends CombatObject
	{		
		private var prototype: StructurePrototype;
		
		public function CombatStructure(combatObjectId: int, type: int, level: int, hp: Number, maxHp: Number)
		{
			super(combatObjectId, type, level, hp, maxHp);
						
			prototype = StructureFactory.getPrototype(type, level);
		}
		
		override public function getIcon():DisplayObjectContainer 
		{
			var icon: DisplayObjectContainer = super.getIcon();
			
			return ObjectFactory.makeSpriteSmall(icon);
		}
		
		public override function get name(): String {
			if (!prototype)
				return "Unknown";
			
			return prototype.getName();
		}				
	}
	
}
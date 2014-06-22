package src.Objects.Battle {
    import flash.display.DisplayObjectContainer;

    import src.Objects.Factories.*;
    import src.Objects.Prototypes.*;

    public class CombatStructure extends CombatObject
	{		
		private var structurePrototype: StructurePrototype;

        public var theme: String;
		
		public function CombatStructure(combatObjectId: int, theme: String, type: int, level: int, hp: Number, maxHp: Number, count: int)
		{
			super(combatObjectId, type, level, hp, maxHp, count);
            this.theme = theme;

            structurePrototype = StructureFactory.getPrototype(type, level);
		}

        override public function getIcon(): DisplayObjectContainer {
            return StructureFactory.getFlashSprite(theme, type, level);
        }

		public override function get name(): String {
			if (!structurePrototype)
				return "Unknown";
			
			return structurePrototype.getName();
		}				
	}
	
}
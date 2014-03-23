package src.Objects.Battle {
    import flash.display.DisplayObjectContainer;

    import src.Objects.Factories.*;
    import src.Objects.Prototypes.*;

    public class CombatStructure extends CombatObject
	{		
		private var structurePrototype: StructurePrototype;

        public var style: String;
		
		public function CombatStructure(combatObjectId: int, style: String, type: int, level: int, hp: Number, maxHp: Number)
		{
			super(combatObjectId, type, level, hp, maxHp);
            this.style = style;

            structurePrototype = StructureFactory.getPrototype(type, level);
		}

        override public function getIcon(): DisplayObjectContainer {
            return StructureFactory.getSprite(style, type, level);
        }

		public override function get name(): String {
			if (!structurePrototype)
				return "Unknown";
			
			return structurePrototype.getName();
		}				
	}
	
}
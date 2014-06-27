package src.Objects.Battle {
    import flash.display.DisplayObjectContainer;

    import src.Objects.Factories.SpriteFactory;

    import src.Objects.Factories.UnitFactory;
    import src.Objects.Prototypes.UnitPrototype;

    public class CombatUnit extends CombatObject
	{
		private var unitPrototype: UnitPrototype;
		
		public function CombatUnit(combatObjectId: int, type: int, level: int, hp: Number, maxHp: Number, count: int)
		{
			super(combatObjectId, type, level, hp, maxHp, count);

			unitPrototype = UnitFactory.getPrototype(type, level);
		}

        override public function getIcon(): DisplayObjectContainer {
            return SpriteFactory.getFlashSprite(UnitFactory.getSpriteName(type, level));
        }
		
		public override function get name(): String {
			if (!unitPrototype)
				return "Unknown";
				
			return unitPrototype.getName();
		}				
	}
	
}
package src.Objects.Battle 
{
    import flash.events.Event;

    public class BattleObjectEvent extends Event
	{
		public var combatObject:CombatObject;
		public var combatGroup:CombatGroup;
		
		public function BattleObjectEvent(type: String, combatGroup: CombatGroup, combatObject: CombatObject) 
		{
			super(type);
			this.combatObject = combatObject;
			this.combatGroup = combatGroup;			
		}
		
	}

}
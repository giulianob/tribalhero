package src.Objects.Battle 
{
    import flash.events.Event;

    public class BattleGroupEvent extends Event
	{
		public var combatGroup:CombatGroup;
		
		public function BattleGroupEvent(type: String, combatGroup: CombatGroup) 
		{
			super(type);
			this.combatGroup = combatGroup;			
		}
		
	}

}
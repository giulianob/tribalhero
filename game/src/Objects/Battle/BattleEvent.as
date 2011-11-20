package src.Objects.Battle {
	import flash.events.Event;
	
	/**
	* ...
	* @author Default
	*/
	public class BattleEvent extends Event
	{
		
		public var combatObj: CombatObject;
		public var destCombatObj: CombatObject;
		public var dmg: int;
		
		public function BattleEvent(type: String, combatObj: CombatObject = null, destCombatObj: CombatObject = null, dmg: int = 0) {
			super(type);
			
			this.combatObj = combatObj;
			this.destCombatObj = destCombatObj;
			this.dmg = dmg;
		}
		
	}
	
}
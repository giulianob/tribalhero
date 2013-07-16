package src.Objects.Battle {
    import flash.events.Event;

    public class BattleAttackEvent extends Event
	{
		public var attackerSide:int;
		public var dmg:Number;
		public var targetCombatObj:CombatObject;
		public var targetCombatGroup:CombatGroup;
		public var attackerCombatObj:CombatObject;
		public var attackerCombatGroup:CombatGroup;
		
		public function BattleAttackEvent(type: String, attackerSide: int, attackerCombatGroup: CombatGroup, attackerCombatObj: CombatObject, targetCombatGroup: CombatGroup, targetCombatObj: CombatObject, dmg: Number = 0) {
			super(type);
			this.attackerSide = attackerSide;
			this.dmg = dmg;
			this.targetCombatObj = targetCombatObj;
			this.targetCombatGroup = targetCombatGroup;
			this.attackerCombatObj = attackerCombatObj;
			this.attackerCombatGroup = attackerCombatGroup;		
		}
		
	}
	
}
package src.Objects.Battle {
    import flash.events.Event;
    import flash.events.EventDispatcher;
    import flash.utils.Dictionary;

    import src.Util.BinaryList.*;
    import src.Util.Util;

    public class BattleManager extends EventDispatcher
	{		
		public static const GROUP_UNIT_REMOVED: String = "BATTLE_GROUP_UNIT_REMOVED";
		public static const GROUP_UNIT_ADDED: String = "BATTLE_GROUP_UNIT_ADDED";
		public static const GROUP_ADDED_ATTACK: String = "BATTLE_GROUP_ADDED_ATTACK";
		public static const GROUP_ADDED_DEFENSE: String = "BATTLE_GROUP_ADDED_DEFENSE";		
		public static const GROUP_REMOVED_ATTACK: String = "BATTLE_GROUP_REMOVED_ATTACK";
		public static const GROUP_REMOVED_DEFENSE: String = "BATTLE_GROUP_REMOVED_DEFENSE";
		public static const OBJECT_ATTACKED: String = "BATTLE_OBJECT_ATTACKED";
		public static const OBJECT_SKIPPED: String = "BATTLE_OBJECT_SKIPPED";
		public static const END: String = "BATTLE_ENDED";		
		public static const NEW_ROUND: String = "BATTLE_NEW_ROUND";		
		public static const PROPERTIES_CHANGED: String = "BATTLE_PROPERTIES_CHANGED";
		
		public static const SIDE_DEFENSE: int = 0;
		public static const SIDE_ATTACK: int = 1;		
		
		public static const STRUCTURE: int = 0;
		public static const UNIT: int = 1;
		
		public var attackers: BinaryList = new BinaryList(CombatGroup.sortOnId, CombatGroup.compareObjId);
		public var defenders: BinaryList = new BinaryList(CombatGroup.sortOnId, CombatGroup.compareObjId);
		
		public var properties: Dictionary = new Dictionary();
		
		public var battleId: int;
		public var location: BattleLocation;
		
		public function BattleManager(battleId: int) {
			this.battleId = battleId;			
		}
		
		public function setProperties(properties: Dictionary): void {
			this.properties = properties;
			dispatchEvent(new Event(PROPERTIES_CHANGED));
		}
		
		public function end(): void {
			attackers.clear();
			defenders.clear();
			
			dispatchEvent(new Event(END));
		}
		
		public function addToDefense(group: CombatGroup): void
		{
			defenders.add(group);
			
			registerGroupEvents(group);
			
			dispatchEvent(new BattleGroupEvent(GROUP_ADDED_DEFENSE, group));
		}
		
		public function addToAttack(group: CombatGroup): void
		{
			attackers.add(group);
			
			registerGroupEvents(group);
			
			dispatchEvent(new BattleGroupEvent(GROUP_ADDED_ATTACK, group));
		}
		
		private function registerGroupEvents(group:CombatGroup): void 
		{
			var localGroup: CombatGroup = group;
			
			group.addEventListener(BinaryListEvent.ADDED, function(e: BinaryListEvent): void {
				dispatchEvent(new BattleObjectEvent(GROUP_UNIT_ADDED, localGroup, e.item));
			}, false, 0, true);
			
			group.addEventListener(BinaryListEvent.REMOVED, function(e: BinaryListEvent): void {
				dispatchEvent(new BattleObjectEvent(GROUP_UNIT_REMOVED, localGroup, e.item));
			}, false, 0, true);			
		}
		
		public function newRound(round: int): void {
			dispatchEvent(new BattleRoundEvent(NEW_ROUND, round));
		}
		
		public function getCombatGroup(combatGroupId: int): CombatGroup {
			return attackers.get(combatGroupId) || defenders.get(combatGroupId);
		}
		
		public function skipped(combatGroupId: int, combatObjectId: int):void
		{
			var group: CombatGroup = getCombatGroup(combatGroupId);
			
			var combatObject: CombatObject = group.get(combatObjectId);
			
			if (combatObject == null)
			{
				Util.log("Received skip command for unknown combat object. combatObject: " + combatObjectId);
				return;
			}
			
			dispatchEvent(new BattleObjectEvent(OBJECT_SKIPPED, group, combatObject));
		}
		
		public function attack(attackingSide: int, attackerGroupId: int, attackerCombatId: int, defenderGroupId: int, defenderCombatId: int, dmg: Number, attackerCount: int, targetCount: int): void
		{
			var attackerGroup: CombatGroup = attackers.get(attackerGroupId) || defenders.get(attackerGroupId);
			var defenderGroup: CombatGroup = attackers.get(defenderGroupId) || defenders.get(defenderGroupId);
			
			if (attackerGroup == null || defenderGroup == null)
			{
				Util.log("Received attack command for unknown combat group. Attacker: " + attackerGroup + " Defender: " + defenderGroup + " Dmg:" + dmg);
				return;
			}
			
			var attacker: CombatObject = attackerGroup.get(attackerCombatId);
			var defender: CombatObject = defenderGroup.get(defenderCombatId);
			
			if (attacker == null || defender == null)
			{
				Util.log("Received attack command for unknown combat object. Attacker: " + attackerCombatId + " Defender: " + defenderCombatId + " Dmg:" + dmg);
				return;
			}

			defender.hp = Util.roundNumber(defender.hp - dmg);
            defender.count = targetCount;
			
			dispatchEvent(new BattleAttackEvent(OBJECT_ATTACKED, attackingSide, attackerGroup, attacker, defenderGroup, defender, dmg, attackerCount, targetCount));
			
			if (defender.hp <= 0)
			{
				defender.hp = 0;
				defenderGroup.remove(defender.combatObjectId);
			}
		}
		
		public function removeFromAttack(groupId: int):void
		{
			var group: CombatGroup = attackers.remove(groupId);
			if (group) {
				dispatchEvent(new BattleGroupEvent(GROUP_REMOVED_ATTACK, group));
			}
		}
		
		public function removeFromDefense(groupId: int):void
		{
			var group: CombatGroup = defenders.remove(groupId);
			if (group) {
				dispatchEvent(new BattleGroupEvent(GROUP_REMOVED_DEFENSE, group));
			}
		}		
	}
	
}
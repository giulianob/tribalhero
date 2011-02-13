package src.Objects.Battle {
	import flash.events.Event;
	import flash.events.EventDispatcher;
	import src.Util.BinaryList.*;
	
	/**
	* ...
	* @author Default
	*/
	public class BattleManager extends EventDispatcher
	{		
		public static const OBJECT_REMOVED_ATTACK: String = "BATTLE_OBJECT_REMOVED_ATTACK";
		public static const OBJECT_REMOVED_DEFENSE: String = "BATTLE_OBJECT_REMOVED_DEFENSE";
		public static const OBJECT_ADDED_ATTACK: String = "BATTLE_OBJECT_ADDED_ATTACK";
		public static const OBJECT_ADDED_DEFENSE: String = "BATTLE_OBJECT_ADDED_DEFENSE";
		public static const OBJECT_ATTACKED: String = "BATTLE_OBJECT_ATTACKED";
		public static const OBJECT_SKIPPED: String = "BATTLE_OBJECT_SKIPPED";
		public static const END: String = "BATTLE_ENDED";		
		public static const NEW_ROUND: String = "BATTLE_NEW_ROUND";		
		
		public static const STRUCTURE: int = 0;
		public static const UNIT: int = 1;
		
		public var attackers: BinaryList = new BinaryList(CombatObject.sortOnId, CombatObject.compareObjId);
		public var defenders: BinaryList = new BinaryList(CombatObject.sortOnId, CombatObject.compareObjId);
		public var all: BinaryList = new BinaryList(CombatObject.sortOnId, CombatObject.compareObjId);
		
		public function BattleManager() {
			
		}
		
		public function end() : void {
			attackers.clear();
			defenders.clear();
			all.clear();
			
			dispatchEvent(new BattleEvent(END));
		}
		
		public function addToDefense(classType: int, playerId: int, cityId: int, combatObjectId: int, troopStubId: int, type: int, level: int, hp: int):void
		{
			var combatObj: CombatObject = add(defenders, classType, playerId, cityId, combatObjectId, troopStubId, type, level, hp);
			
			dispatchEvent(new BattleEvent(OBJECT_ADDED_DEFENSE, combatObj));
		}
		
		public function addToAttack(classType: int, playerId: int, cityId: int, combatObjectId: int, troopStubId: int, type: int, level: int, hp: int):void
		{
			var combatObj: CombatObject = add(attackers, classType, playerId, cityId, combatObjectId, troopStubId, type, level, hp);
			
			dispatchEvent(new BattleEvent(OBJECT_ADDED_ATTACK, combatObj));
		}
		
		private function add(list: BinaryList, classType: int, playerId: int, cityId: int, combatObjectId: int, troopStubId: int, type: int, level: int, hp: int): CombatObject
		{
			var combatObj: CombatObject;
			
			if (classType == UNIT)
				combatObj = new CombatUnit(playerId, cityId, combatObjectId, troopStubId, type, level, hp);
			else if (classType == STRUCTURE)
				combatObj = new CombatStructure(playerId, cityId, combatObjectId, troopStubId, type, level, hp);
			else
				throw new Error("Unknown class type " + classType);
				
			list.add(combatObj);
			all.add(combatObj);
			
			return combatObj;
		}
		
		public function newRound(round: int): void {
			dispatchEvent(new BattleRoundEvent(NEW_ROUND, round));
		}
		
		public function skipped(attackerCombatId: int):void
		{
			var attacker: CombatObject = all.get(attackerCombatId);			
			
			if (attacker == null)
			{
				trace("Received skip command for unknown combat object. Attacker: " + attackerCombatId);
				return;
			}
						
			dispatchEvent(new BattleEvent(OBJECT_SKIPPED, attacker, null, 0));		
		}
		
		public function attack(attackerCombatId: int, defenderCombatId: int, dmg: int):void
		{
			var attacker: CombatObject = all.get(attackerCombatId);
			var defender: CombatObject = all.get(defenderCombatId);
			
			if (attacker == null || defender == null)
			{
				trace("Received attack command for unknown combat object. Attacker: " + attackerCombatId + " Defender: " + defenderCombatId + " Dmg:" + dmg);
				return;
			}
				
			defender.hp -= dmg;
			
			dispatchEvent(new BattleEvent(OBJECT_ATTACKED, attacker, defender, dmg));			
			
			if (defender.hp <= 0)
			{
				defender.hp = 0;	
				all.remove(defenderCombatId);
				
				if (attackers.remove(defenderCombatId))
					dispatchEvent(new BattleEvent(OBJECT_REMOVED_ATTACK, defender));
				else if (defenders.remove(defenderCombatId))
					dispatchEvent(new BattleEvent(OBJECT_REMOVED_DEFENSE, defender));
			}
		}

	}
	
}
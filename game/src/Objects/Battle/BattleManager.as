package src.Objects.Battle {
	import flash.events.Event;
	import flash.events.EventDispatcher;
	import src.Util.BinaryList.*;
	import src.Util.Util;
	
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
		public var battleId: int;
		public var cityId: int;
		
		public function BattleManager(cityId: int) {
			this.cityId = cityId;
		}
		
		public function end() : void {
			attackers.clear();
			defenders.clear();
			all.clear();
			
			dispatchEvent(new BattleEvent(END));
		}
		
		public function addToDefense(classType: int, playerId: int, cityId: int, combatObjectId: int, troopStubId: int, type: int, level: int, hp: Number, maxHp: Number):void
		{
			var combatObj: CombatObject = add(defenders, classType, playerId, cityId, combatObjectId, troopStubId, type, level, hp, maxHp);
			
			dispatchEvent(new BattleEvent(OBJECT_ADDED_DEFENSE, combatObj));
		}
		
		public function addToAttack(classType: int, playerId: int, cityId: int, combatObjectId: int, troopStubId: int, type: int, level: int, hp: Number, maxHp: Number):void
		{
			var combatObj: CombatObject = add(attackers, classType, playerId, cityId, combatObjectId, troopStubId, type, level, hp, maxHp);
			
			dispatchEvent(new BattleEvent(OBJECT_ADDED_ATTACK, combatObj));
		}
		
		private function add(list: BinaryList, classType: int, playerId: int, cityId: int, combatObjectId: int, troopStubId: int, type: int, level: int, hp: Number, maxHp: Number): CombatObject
		{
			var combatObj: CombatObject;
			
			if (classType == UNIT)
				combatObj = new CombatUnit(playerId, cityId, combatObjectId, troopStubId, type, level, hp, maxHp);
			else if (classType == STRUCTURE)
				combatObj = new CombatStructure(playerId, cityId, combatObjectId, troopStubId, type, level, hp, maxHp);
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
				Util.log("Received skip command for unknown combat object. Attacker: " + attackerCombatId);
				return;
			}
						
			dispatchEvent(new BattleEvent(OBJECT_SKIPPED, attacker, null, 0));		
		}
		
		public function attack(attackerCombatId: int, defenderCombatId: int, dmg: Number):void
		{
			var attacker: CombatObject = all.get(attackerCombatId);
			var defender: CombatObject = all.get(defenderCombatId);
			
			if (attacker == null || defender == null)
			{
				Util.log("Received attack command for unknown combat object. Attacker: " + attackerCombatId + " Defender: " + defenderCombatId + " Dmg:" + dmg);
				return;
			}
				
			defender.hp -= dmg;
			defender.hp = Util.roundNumber(defender.hp);
			
			dispatchEvent(new BattleEvent(OBJECT_ATTACKED, attacker, defender, dmg));			
			
			if (defender.hp <= 0)
			{
				defender.hp = 0;
				removeObject(defender);
			}
		}

		public function removeFromAttack(cityId: int, troopId: int):void
		{
			remove(attackers, cityId, troopId);
		}
		
		public function removeFromDefense(cityId: int, troopId: int):void
		{
			remove(defenders, cityId, troopId);
		}
		
		private function remove(list: BinaryList, cityId: int, troopId: int):void
		{
			var objs: Array = new Array();
			for each(var co:CombatObject in list.each())
				if (co.cityId == cityId && co.troopStubId == troopId)
					objs.push(co);

			for each(var co2:CombatObject in objs)
				removeObject(co2);
		}
		
		private function removeObject(co: CombatObject):void
		{
			all.remove(co);
			if (attackers.remove(co.combatObjectId))
				dispatchEvent(new BattleEvent(OBJECT_REMOVED_ATTACK, co));
			else if (defenders.remove(co.combatObjectId))
				dispatchEvent(new BattleEvent(OBJECT_REMOVED_DEFENSE, co));
		}
	}
	
}
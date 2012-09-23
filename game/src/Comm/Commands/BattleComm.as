﻿package src.Comm.Commands {

	import src.Comm.*;
	import src.GameContainer;
	import src.Global;
	import src.Map.MapComm;
	import src.Objects.Battle.BattleLocation;
	import src.Objects.Battle.BattleManager;
	import src.Objects.Battle.BattleObjectEvent;
	import src.Objects.Battle.BattleOwner;
	import src.Objects.Battle.CombatGroup;
	import src.Objects.Battle.CombatObject;
	import src.Objects.Battle.CombatStructure;
	import src.Objects.Battle.CombatUnit;
	import src.Objects.GameError;
	import src.UI.Dialog.BattleViewer;
	import src.UI.Dialog.InfoDialog;
	import src.Util.Util;

	public class BattleComm {

		private var battles: Array = new Array();
		private var mapComm: MapComm;
		private var session: Session;

		public function BattleComm(mapComm: MapComm) {
			this.mapComm = mapComm;
			this.session = mapComm.session;

			session.addEventListener(Commands.CHANNEL_NOTIFICATION, onChannelReceive);
		}
		
		public function dispose() : void {
			session.removeEventListener(Commands.CHANNEL_NOTIFICATION, onChannelReceive);
		}
		
		private function getBattle(battleId: int): BattleManager {
			for each (var battle: BattleManager in battles) {
				if (battle.battleId == battleId) 
				{
					return battle;
				}
			}
			
			return null;
		}
		
		public function onChannelReceive(e: PacketEvent):void
		{
			switch(e.packet.cmd)
			{
				case Commands.BATTLE_ATTACK:
					onReceiveBattleAttack(e.packet);
				break;
				case Commands.BATTLE_REINFORCE_ATTACKER:
				case Commands.BATTLE_REINFORCE_DEFENDER:
					onReceiveBattleReinforce(e.packet);
				break;
				case Commands.BATTLE_ENDED:
					onReceiveBattleEnded(e.packet);
				break;
				case Commands.BATTLE_SKIPPED:
					onReceiveBattleSkipped(e.packet);					
				break;
				case Commands.BATTLE_NEW_ROUND:
					onReceiveNewRound(e.packet);
				break;
				case Commands.BATTLE_WITHDRAW_ATTACKER:
					onReceiveWithdrawAttacker(e.packet);
				break;
				case Commands.BATTLE_WITHDRAW_DEFENDER:
					onReceiveWithdrawDefender(e.packet);
				break;
				case Commands.BATTLE_GROUP_UNIT_ADDED:
					onReceiveGroupUnitAdded(e.packet);				
				break;
				case Commands.BATTLE_GROUP_UNIT_REMOVED:
					onReceiveGroupUnitRemoved(e.packet);				
				break;				
			}
		}
		
		public function battleSubscribe(battleId: int, battleViewer: BattleViewer): BattleManager
		{
			var battle: BattleManager = getBattle(battleId);
			if (battle) {
				return null;
			}
			
			var packet: Packet = new Packet();
			packet.cmd = Commands.BATTLE_SUBSCRIBE;
			packet.writeUInt(battleId);

			mapComm.session.write(packet, onReceiveBattleSubscribe, { battleViewer: battleViewer} );

			battle = new BattleManager(battleId);
			
			battles.push(battle);

			return battle;
		}

		public function onReceiveBattleSubscribe(packet: Packet, custom: *):void
		{		
			var battleId: int = packet.readUInt();
			
			var battle: BattleManager = getBattle(battleId);

			if ((packet.option & Packet.OPTIONS_FAILED) == Packet.OPTIONS_FAILED)
			{			
				battle.end();
				battles.splice(battles.indexOf(battle), 1);
				
				if (custom.battleViewer.getFrame()) {
					custom.battleViewer.getFrame().dispose();
				}

				var err: int = packet.readUInt();
				var roundsLeft: int = packet.readInt();

				InfoDialog.showMessageDialog("Battle", GameError.getMessage(err) + (roundsLeft > 0 ? " Battle will be viewable in approximately " + roundsLeft + " round(s)." : ""));

				return;
			}

			// Hide the sidebar from the selected troop
			Global.gameContainer.clearAllSelections();
			
			var locationType: int = packet.readUByte();
			var locationId: int = packet.readUInt();
			var locationName: String = packet.readString();
			battle.location = new BattleLocation(locationType, locationId, locationName);
			
			var round: int = packet.readUInt();
			battle.newRound(round);
			
			// Properties
			var propertiesCount: int = packet.readByte();
			for (var i: int = 0; i < propertiesCount; i++) {
				battle.properties[packet.readString()] = packet.readString();
			}
			
			// Add units
			var group: CombatGroup;
			
			var attackerGroups: int = packet.readInt();
			for (i = 0; i < attackerGroups; i++) {
				group = readGroup(packet);
				battle.addToAttack(group);
			}

			var defendersGroup: int = packet.readInt();
			for (i = 0; i < defendersGroup; i++) {
				group = readGroup(packet);
				battle.addToDefense(group);
			}			
		}
		
		private function readGroup(packet: Packet): CombatGroup {
			var combatGroupId: int = packet.readUInt();
			var troopId: int = packet.readUByte();
			var ownerType: int = packet.readUByte();
			var ownerId: int = packet.readUInt();
			var ownerName: String = packet.readString();
			
			var group: CombatGroup = new CombatGroup(combatGroupId, troopId, new BattleOwner(ownerType, ownerId, ownerName));
						
			var objCount: int = packet.readInt();
			
			for (var i: int = 0; i < objCount; i++) {			
				var combatObj: CombatObject = readCombatObject(packet);
				group.add(combatObj);				
			}
			
			return group;
		}
		
		private function readCombatObject(packet: Packet): CombatObject {
			var combatObjId: int = packet.readUInt();
			var classType: int = packet.readUByte();
			var type: int = packet.readUShort();
			var level: int = packet.readUByte();
			var hp: Number = packet.readFloat();
			var maxHp: Number = packet.readFloat();
			
			var combatObj: CombatObject;
			if (classType == BattleManager.UNIT) {
				combatObj = new CombatUnit(combatObjId, type, level, hp, maxHp);
			}
			else if (classType == BattleManager.STRUCTURE) {
				combatObj = new CombatStructure(combatObjId, type, level, hp, maxHp);
			}
			else {
				throw new Error("Unknown class type " + classType);
			}
			
			return combatObj;
		}

		public function onReceiveBattleEnded(packet: Packet) : void {
			var battle: BattleManager = getBattle(packet.readUInt());
			if (!battle) return;
			
			battles.splice(battles.indexOf(battle), 1);
			battle.end();			
		}
		
		private function onReceiveGroupUnitAdded(packet:Packet):void 
		{
			var battle: BattleManager = getBattle(packet.readUInt());
			if (!battle) return;
			
			var group: CombatGroup = battle.getCombatGroup(packet.readUInt());
			
			var combatObj: CombatObject = readCombatObject(packet);
			
			group.add(combatObj);
		}		
		
		private function onReceiveGroupUnitRemoved(packet:Packet):void 
		{
			var battle: BattleManager = getBattle(packet.readUInt());
			if (!battle) return;
			
			var group: CombatGroup = battle.getCombatGroup(packet.readUInt());
			
			group.remove(packet.readUInt());
		}

		public function onReceiveBattleReinforce(packet: Packet):void
		{
			var battle: BattleManager = getBattle(packet.readUInt());
			if (!battle) return;
			
			var group: CombatGroup = readGroup(packet);

			if (packet.cmd == Commands.BATTLE_REINFORCE_ATTACKER) {
				battle.addToAttack(group);
			}
			else {
				battle.addToDefense(group);
			}
		}

		public function battleUnsubscribe(battleId: int):void
		{
			var battle: BattleManager = getBattle(battleId);
			
			if (battle) {
				battles.splice(battles.indexOf(battle), 1);
			}
			
			var packet: Packet = new Packet();
			packet.cmd = Commands.BATTLE_UNSUBSCRIBE;
			packet.writeUInt(battleId);

			mapComm.session.write(packet);
		}

		public function onReceiveBattleAttack(packet: Packet):void
		{	
			var battle: BattleManager = getBattle(packet.readUInt());
			if (!battle) return;			
			
			var side: int = packet.readUByte();
			var attackerGroupId: int = packet.readUInt();
			var attackerObjId: int = packet.readUInt();
			var defenderGroupId: int = packet.readUInt();
			var defenderObjId: int = packet.readUInt();
			var dmg: Number = Util.roundNumber(packet.readFloat());

			battle.attack(side, attackerGroupId, attackerObjId, defenderGroupId, defenderObjId, dmg);
		}

		public function onReceiveNewRound(packet: Packet):void
		{
			var battle: BattleManager = getBattle(packet.readUInt());
			if (!battle) return;
			
			battle.newRound(packet.readUInt());
		}
	
		public function onReceiveBattleSkipped(packet: Packet):void
		{
			var battle: BattleManager = getBattle(packet.readUInt());
			if (!battle) return;
			
			battle.skipped(packet.readUInt(), packet.readUInt());
		}
		
		public function onReceiveWithdrawAttacker(packet: Packet):void
		{
			var battle: BattleManager = getBattle(packet.readUInt());
			if (!battle) return;

			battle.removeFromAttack(packet.readUInt());
		}
		
		public function onReceiveWithdrawDefender(packet: Packet):void
		{
			var battle: BattleManager = getBattle(packet.readUInt());
			if (!battle) return;

			battle.removeFromDefense(packet.readUInt());
		}
	}

}


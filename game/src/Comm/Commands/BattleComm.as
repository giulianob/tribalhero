package src.Comm.Commands {

	import src.Comm.*;
	import src.GameContainer;
	import src.Global;
	import src.Map.MapComm;
	import src.Objects.Battle.BattleManager;
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
		
		private function getBattleByCityId(subscribedCityId: int): BattleManager {
			for each (var eachBattle: BattleManager in battles) {
				if (eachBattle.cityId == subscribedCityId) {
					return eachBattle;
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
			}
		}

		public function battleSubscribe(cityId: int, battleViewer: BattleViewer): BattleManager
		{
			var battle: BattleManager = getBattleByCityId(cityId);
			if (battle) {
				return null;
			}
			
			var packet: Packet = new Packet();
			packet.cmd = Commands.BATTLE_SUBSCRIBE;
			packet.writeUInt(cityId);

			mapComm.session.write(packet, onReceiveBattleSubscribe, {cityId: cityId, battleViewer: battleViewer} );

			battle = new BattleManager(cityId);
			
			battles.push(battle);

			return battle;
		}

		public function onReceiveBattleSubscribe(packet: Packet, custom: *):void
		{
			var subscribeCityId: int = custom.cityId;
			
			var battle: BattleManager = getBattleByCityId(subscribeCityId);

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

			// Set battle id
			battle.battleId = packet.readUInt();
			
			battle.newRound(packet.readUInt());
			
			// Add units
			var playerId: int;
			var cityId: int;
			var combatObjId: int;
			var classType: int;
			var type: int;
			var troopStubId: int;
			var level: int;
			var hp: Number;
			var maxHp: Number;

			var attackersCnt: int = packet.readUShort();
			for (var i: int = 0; i < attackersCnt; i++)
			{
				playerId = packet.readUInt();
				cityId = packet.readUInt();
				combatObjId = packet.readUInt();
				classType = packet.readUByte();
				troopStubId = packet.readUByte();
				type = packet.readUShort();
				level = packet.readUByte();
				hp = packet.readFloat();
				maxHp = packet.readFloat();

				battle.addToAttack(classType, playerId, cityId, combatObjId, troopStubId, type, level, hp, maxHp);
			}

			var defendersCnt: int = packet.readUShort();
			for (i = 0; i < defendersCnt; i++)
			{
				playerId = packet.readUInt();
				cityId = packet.readUInt();
				combatObjId = packet.readUInt();
				classType = packet.readUByte();
				troopStubId = packet.readUByte();
				type = packet.readUShort();
				level = packet.readUByte();
				hp = packet.readFloat();
				maxHp = packet.readFloat();

				battle.addToDefense(classType, playerId, cityId, combatObjId, troopStubId, type, level, hp, maxHp);
			}
		}

		public function onReceiveBattleEnded(packet: Packet) : void {
			var battle: BattleManager = getBattle(packet.readUInt());
			if (!battle) return;
			
			battles.splice(battles.indexOf(battle), 1);
			battle.end();			
		}

		public function onReceiveBattleReinforce(packet: Packet):void
		{
			var battle: BattleManager = getBattle(packet.readUInt());
			if (!battle) return;

			var playerId: int;
			var cityId: int;
			var combatObjId: int;
			var classType: int;
			var type: int;
			var troopStubId: int;
			var level: int;
			var hp: Number;
			var maxHp: Number;

			var cnt: int = packet.readUShort();
			for (var i: int = 0; i < cnt; i++)
			{
				playerId = packet.readUInt();
				cityId = packet.readUInt();
				combatObjId = packet.readUInt();
				classType = packet.readUByte();
				troopStubId = packet.readUByte();
				type = packet.readUShort();
				level = packet.readUByte();
				hp = packet.readFloat();
				maxHp = packet.readFloat();

				if (packet.cmd == Commands.BATTLE_REINFORCE_ATTACKER)
					battle.addToAttack(classType, playerId, cityId, combatObjId, troopStubId, type, level, hp, maxHp);
				else
					battle.addToDefense(classType, playerId, cityId, combatObjId, troopStubId, type, level, hp, maxHp);
			}
		}

		public function battleUnsubscribe(cityId: int):void
		{
			var battle: BattleManager = getBattleByCityId(cityId);
			
			if (battle) {
				battles.splice(battles.indexOf(battle), 1);
			}
			
			var packet: Packet = new Packet();
			packet.cmd = Commands.BATTLE_UNSUBSCRIBE;
			packet.writeUInt(cityId);

			mapComm.session.write(packet);
		}

		public function onReceiveBattleAttack(packet: Packet):void
		{	
			var battle: BattleManager = getBattle(packet.readUInt());
			if (!battle) return;			
					
			var attackerObjId: int = packet.readUInt();
			var defenderObjId: int = packet.readUInt();
			var dmg: Number = Util.roundNumber(packet.readFloat());

			battle.attack(attackerObjId, defenderObjId, dmg);
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
			
			var attackerObjId: int = packet.readUInt();

			battle.skipped(attackerObjId);
		}
		
		public function onReceiveWithdrawAttacker(packet: Packet):void
		{
			var battle: BattleManager = getBattle(packet.readUInt());
			if (!battle) return;

			var cityId: int = packet.readUInt();
			var stubId: int = packet.readByte();
			battle.removeFromAttack(cityId, stubId);
		}
		
		public function onReceiveWithdrawDefender(packet: Packet):void
		{
			var battle: BattleManager = getBattle(packet.readUInt());
			if (!battle) return;

			var cityId: int = packet.readUInt();
			var stubId: int = packet.readByte();
			battle.removeFromDefense(cityId, stubId);
		}
	}

}


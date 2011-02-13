package src.Comm.Commands {

	import src.Comm.*;
	import src.Global;
	import src.Map.MapComm;
	import src.Objects.Battle.BattleManager;
	import src.Objects.GameError;
	import src.UI.Dialog.InfoDialog;

	public class BattleComm {

		private var battle: BattleManager;
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
			}
		}

		public function battleSubscribe(cityId: int): BattleManager
		{
			if (battle)
			{
				return null; //we shouldnt have a battle at this point
			}

			var packet: Packet = new Packet();
			packet.cmd = Commands.BATTLE_SUBSCRIBE;
			packet.writeUInt(cityId);

			mapComm.session.write(packet, onReceiveBattleSubscribe, cityId);

			battle = new BattleManager();

			return battle;
		}

		public function onReceiveBattleSubscribe(packet: Packet, custom: *):void
		{
			if ((packet.option & Packet.OPTIONS_FAILED) == Packet.OPTIONS_FAILED)
			{
				battle.end();
				battle = null;

				var err: int = packet.readUInt();
				var roundsLeft: int = packet.readInt();

				InfoDialog.showMessageDialog("Battle", GameError.getMessage(err) + (roundsLeft > 0 ? " Battle will be viewable in approximately " + roundsLeft + " rounds." : ""));				

				return;
			}

			// Hide the sidebar from the selected troop
			Global.gameContainer.setSidebar(null);
			
			var subscribeCityId: int = custom;

			var playerId: int;
			var cityId: int;
			var combatObjId: int;
			var classType: int;
			var type: int;
			var troopStubId: int;
			var level: int;
			var hp: int;

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
				hp = packet.readUInt();

				battle.addToAttack(classType, playerId, cityId, combatObjId, troopStubId, type, level, hp);
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
				hp = packet.readUInt();

				battle.addToDefense(classType, playerId, cityId, combatObjId, troopStubId, type, level, hp);
			}
		}

		public function onReceiveBattleEnded(packet: Packet) : void {
			if (battle == null) return;

			battle.end();
		}

		public function onReceiveBattleReinforce(packet: Packet):void
		{
			if (battle == null) return;

			var playerId: int;
			var cityId: int;
			var combatObjId: int;
			var classType: int;
			var type: int;
			var troopStubId: int;
			var level: int;
			var hp: int;

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
				hp = packet.readUInt();

				if (packet.cmd == Commands.BATTLE_REINFORCE_ATTACKER)
				battle.addToAttack(classType, playerId, cityId, combatObjId, troopStubId, type, level, hp);
				else
				battle.addToDefense(classType, playerId, cityId, combatObjId, troopStubId, type, level, hp);
			}
		}

		public function battleUnsubscribe(cityId: int):void
		{
			battle = null;

			var packet: Packet = new Packet();
			packet.cmd = Commands.BATTLE_UNSUBSCRIBE;
			packet.writeUInt(cityId);

			mapComm.session.write(packet);
		}

		public function onReceveBattleUnsubscribe(packet: Packet, custom: * ):void
		{
			battle = null;
		}

		public function onReceiveBattleAttack(packet: Packet):void
		{
			if (battle == null) return;

			var attackerObjId: int = packet.readUInt();
			var defenderObjId: int = packet.readUInt();
			var dmg: int = packet.readUShort();

			battle.attack(attackerObjId, defenderObjId, dmg);
		}

		public function onReceiveNewRound(packet: Packet):void
		{
			if (battle == null) return;		
			
			battle.newRound(packet.readUInt());
		}
	
		public function onReceiveBattleSkipped(packet: Packet):void
		{
			if (battle == null) return;		

			var attackerObjId: int = packet.readUInt();

			battle.skipped(attackerObjId);
		}

	}

}


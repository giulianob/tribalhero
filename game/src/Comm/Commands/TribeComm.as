package src.Comm.Commands {

	import flash.utils.ByteArray;
	import src.Comm.*;
	import src.Constants;
	import src.Map.*;
	import src.Objects.*;
	import src.Objects.Actions.*;
	import src.Objects.Troop.*;
	import src.Global;
	import src.UI.Components.TroopStubGridList.TroopStubGridCell;
	import src.UI.Dialog.InfoDialog;
	import src.UI.Dialog.TribeProfileDialog;
	import src.Util.*;

	public class TribeComm {

		private var mapComm: MapComm;
		private var session: Session;

		public function TribeComm(mapComm: MapComm) {
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
				case Commands.TRIBE_UPDATE_CHANNEL:
					onReceiveTribeUpdate(e.packet);
				break;
			}
		}

		public function onReceiveTribeUpdate(packet: Packet) : void {
			Constants.tribeId = packet.readUInt();
			Constants.tribeInviteId = packet.readUInt();
			Constants.tribeRank = packet.readUByte();
			Global.gameContainer.tribeInviteRequest.visible = Constants.tribeInviteId > 0;
		}
		
		public function contribute(cityId: int, resources: Resources, callback: Function): void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.TRIBESMAN_CONTRIBUTE;
			packet.writeUInt(cityId);
			packet.writeInt(resources.crop);
			packet.writeInt(resources.gold);
			packet.writeInt(resources.iron);
			packet.writeInt(resources.wood);
			
			mapComm.showLoading();
			session.write(packet, mapComm.catchAllErrors, callback);
		}
		
		public function createTribe(name: String) : void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.TRIBE_CREATE;
			packet.writeString(name);

			session.write(packet, mapComm.catchAllErrors);
		}	
		
		public function onCreateTribe(packet:Packet, custom:*) : void {
			if (MapComm.tryShowError(packet)) return;
			Global.gameContainer.tribeTimer.start();
		}

		public function invitationConfirm(response: Boolean) : void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.TRIBESMAN_CONFIRM;
			packet.writeByte(response ? 1 : 0);

			session.write(packet, showErrorOrRefreshTribePanel);
			Global.gameContainer.tribeTimer.start();
		}		
		
		public function viewTribeProfile(callback: Function):void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.TRIBE_INFO;

			session.write(packet, onReceiveTribeProfile, {callback: callback});
		}
		
		public function setTribeDescription(description: String) : void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.TRIBE_DESCRIPTION_SET;
			packet.writeString(description);

			session.write(packet, showErrorOrRefreshTribePanel, { refresh: true });
		}		
		
		public function onReceiveTribeProfile(packet: Packet, custom: *):void {
			if (MapComm.tryShowError(packet)) {
				custom.callback(null);
				return;
			}
			
			var profileData: * = new Object();
			profileData.tribeId = packet.readUInt();
			profileData.chiefId = packet.readUInt();
			profileData.tribeLevel = packet.readUByte();
			profileData.tribeName = packet.readString();
			profileData.description = packet.readString();
			
			profileData.resources = new Resources(packet.readUInt(), packet.readUInt(), packet.readUInt(), packet.readUInt(), 0);
			
			profileData.members = [];
			var memberCount: int = packet.readShort();
			for (var i: int = 0; i < memberCount; i++)
				profileData.members.push({
					playerId: packet.readUInt(),
					playerName: packet.readString(),
					cityCount: packet.readInt(),
					rank: packet.readUByte(),
					date: Util.simpleTime(Global.map.getServerTime()-packet.readUInt()) + " ago",
					contribution: new Resources(packet.readUInt(), packet.readUInt(), packet.readUInt(), packet.readUInt(), 0)
				});
				
			(profileData.members as Array).sortOn("rank", [Array.NUMERIC]);
			
			profileData.incomingAttacks = [];
			var incomingAttackCount: int = packet.readShort();
			for (i = 0; i < incomingAttackCount; i++) {
				profileData.incomingAttacks.push( {
					targetPlayerId: packet.readUInt(),
					targetCityId: packet.readUInt(),
					targetPlayerName: packet.readString(),
					targetCityName: packet.readString(),
					sourcePlayerId: packet.readUInt(),
					sourceCityId: packet.readUInt(),
					sourcePlayerName: packet.readString(),
					sourceCityName: packet.readString(),
					endTime: packet.readUInt()
				});
			}
			
			profileData.assignments = [];
			var assignmentCount: int = packet.readShort();
			for (i = 0; i < assignmentCount; i++) {
				var assignment: * = {
					id: packet.readInt(),
					endTime: packet.readUInt(),
					x: packet.readUInt(),
					y: packet.readUInt(),
					targetPlayerId: packet.readUInt(),
					targetCityId: packet.readUInt(),
					targetPlayerName: packet.readString(),
					targetCityName: packet.readString(),
					attackMode: packet.readByte(),
					dispatchCount: packet.readUInt(),
					troopCount: packet.readInt(),
					troops: []
				};
				
				Global.map.usernames.players.add(new Username(assignment.targetPlayerId, assignment.targetPlayerName));
				Global.map.usernames.cities.add(new Username(assignment.targetCityId, assignment.targetCityName));
				
				for (var assignmentIter: int = 0; assignmentIter < assignment.troopCount; assignmentIter++) {
					var troop: * = {
						playerId: packet.readUInt(),
						cityId: packet.readUInt(),
						playerName: packet.readString(),
						cityName: packet.readString(),
						stub: null
					};
					
					troop.stub = new TroopStub(packet.readByte(), troop.playerId, troop.cityId);
					
					Global.map.usernames.players.add(new Username(troop.playerId, troop.playerName));
					Global.map.usernames.cities.add(new Username(troop.cityId, troop.cityName));
					
					var stub: TroopStub = troop.stub;
					
					var formationCnt: int = packet.readByte();
					for (var formationIter: int = 0; formationIter < formationCnt; formationIter++) {
						var formation: Formation = new Formation(packet.readByte());
						
						var unitCount: int = packet.readByte();
						for (var unitIter: int = 0; unitIter < unitCount; unitIter++) {
							formation.add(new Unit(packet.readUShort(), packet.readUShort()));
						}
						
						stub.add(formation);
					}
					
					assignment.troops.push(troop);
				}
				
				profileData.assignments.push(assignment);
			}
			
			custom.callback(profileData);
		}
		
		public function viewTribePublicProfile(tribe: *, callback: Function):void {
			var packet: Packet = new Packet();
			
			packet.cmd = Commands.TRIBE_PUBLIC_INFO;
			packet.writeUInt(tribe.tribeId);

			session.write(packet, onReceiveTribePublicProfile, { tribe: tribe, callback: callback } );
		}	
		
		public function onReceiveTribePublicProfile(packet: Packet, custom: *):void {
			if (MapComm.tryShowError(packet)) {
				custom.callback(null);
				return;
			}
			
			var profileData: * = new Object();
			profileData.tribeId = custom.tribe.tribeId;
			profileData.tribeName = custom.tribe.tribeName;
			profileData.members = [];
			var memberCount: int = packet.readShort();
			for (var i: int = 0; i < memberCount; i++)
				profileData.members.push({
					playerId: packet.readUInt(),
					playerName: packet.readString(),
					cityCount: packet.readInt(),
					rank: packet.readUByte()
				});
				
			(profileData.members as Array).sortOn("rank", [Array.NUMERIC]);
			custom.callback(profileData);
		}
		
		public function setRank(playerId: int, newRank: int) : void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.TRIBESMAN_SET_RANK;
			packet.writeUInt(playerId);
			packet.writeUByte(newRank);
			
			session.write(packet, showErrorOrRefreshTribePanel, { refresh: true });
		}

		public function kick(playerId: int) : void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.TRIBESMAN_REMOVE;
			packet.writeUInt(playerId);
			
			session.write(packet, showErrorOrRefreshTribePanel, { refresh: true });
		}
		
		public function invitePlayer(playerName: String) : void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.TRIBESMAN_REQUEST;
			packet.writeString(playerName);
			
			mapComm.showLoading();
			session.write(packet, showErrorOrRefreshTribePanel, { message: { title: "Invitation sent", content: "An invitation has been sent to this player to join your tribe." }, refresh: false });
		}
		
		public function dismantle() : void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.TRIBE_DELETE;
			
			session.write(packet, showErrorOrRefreshTribePanel, { message: { title: "Tribe dismantled", content: "Your tribe has been dismantled" }, close: true } );
		}	

		public function leave() : void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.TRIBESMAN_LEAVE;
			
			session.write(packet, showErrorOrRefreshTribePanel, { message: { title: "Tribe", content: "You have left the tribe" }, close: true });
		}			
		
		public function upgrade() : void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.TRIBE_UPGRADE;
			
			session.write(packet, showErrorOrRefreshTribePanel, { refresh: true });
		}
		
		public function showErrorOrRefreshTribePanel(packet: Packet, custom: *): void {
			mapComm.hideLoading();
			
			if (MapComm.tryShowError(packet))
				return;			
			
			if (!custom)
				custom = new Object();
				
			var tribeProfileDialog: TribeProfileDialog = Global.gameContainer.findDialog(TribeProfileDialog); 
			if (tribeProfileDialog) {
				if (custom.close)
					tribeProfileDialog.getFrame().dispose();
				else if (custom.refresh)
					tribeProfileDialog.update();
			}
			
			if (custom.message)
				InfoDialog.showMessageDialog(custom.message.title, custom.message.content);
		}
		
		public function incomingCount(callback : Function):void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.TRIBE_INCOMING_COUNT;
			session.write(packet, onReceiveIncomingCount, { callback: callback } );
		}
		public function onReceiveIncomingCount(packet: Packet, custom: * ):void {
			if ((packet.option & Packet.OPTIONS_FAILED) == Packet.OPTIONS_FAILED) {
				Global.gameContainer.tribeTimer.stop();
				return;
			}
			custom.callback(packet.readInt());
		}
	}

}


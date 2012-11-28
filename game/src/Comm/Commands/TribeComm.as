package src.Comm.Commands {

	import src.*;
	import src.Comm.*;
	import src.Map.*;
	import src.Objects.*;
	import src.Objects.Actions.*;
	import src.Objects.Stronghold.Stronghold;
	import src.Objects.Troop.*;
	import src.UI.Components.ScreenMessages.*;
	import src.UI.Dialog.*;
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
				case Commands.TRIBE_UPDATE_NOTIFICATIONS:
					onReceiveNotifications(e.packet, null);
					break;
				case Commands.TRIBESMAN_GOT_KICKED:
					onTribeLeave();
					break;
			}
		}

		public function onReceiveTribeUpdate(packet: Packet) : void {
			Constants.tribeId = packet.readUInt();
			Constants.tribeInviteId = packet.readUInt();
			Constants.tribeRank = packet.readUByte();
			Global.gameContainer.tribeNotificationIcon.visible = Constants.tribeInviteId > 0;
		}
		
		public function contribute(cityId: int, structureId: int, resources: Resources, callback: Function): void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.TRIBESMAN_CONTRIBUTE;
			packet.writeUInt(cityId);
			packet.writeUInt(structureId);
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
		
		public function invitationConfirm(response: Boolean) : void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.TRIBESMAN_CONFIRM;
			packet.writeByte(response ? 1 : 0);
			session.write(packet, onInvitationConfirm,{response:response});
		}
		
		public function onInvitationConfirm(packet: Packet, custom: * ):void {
			if (MapComm.tryShowError(packet))
				return;
			if(custom.response) {
				var incoming: int = packet.readInt();
				var assignment: int = packet.readShort();
				onTribeJoin(assignment, incoming);
			}
		}
		
		public function setTribeDescription(description: String) : void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.TRIBE_DESCRIPTION_SET;
			packet.writeString(description);

			mapComm.showLoading();
			session.write(packet, showErrorOrRefreshTribePanel, { refresh: true });
		}		
		
		public function viewTribeProfileByName(name: String , callback: Function = null):void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.TRIBE_INFO_BY_NAME;
			packet.writeString(name);
			session.write(packet, onReceiveTribeProfile, {callback: callback});
		}
		
		public function onReceiveTribeProfile(packet: Packet, custom: * ): void {
			mapComm.hideLoading();
			
			if (MapComm.tryShowError(packet)) {
				if(custom.callback!=null) custom.callback(null);
				return;
			}
			var isPrivate: Boolean = packet.readByte()==1;
			if (isPrivate) {
				readPrivateTribeProfile(packet, custom);
			} else {
				readPublicTribeProfile(packet, custom);
			}
		}
		
		public function viewTribeProfile(tribeId: int, callback: Function = null):void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.TRIBE_INFO;
			packet.writeUInt(tribeId);

			session.write(packet, onReceiveTribeProfile, {callback: callback});
		}

		public function readPrivateTribeProfile(packet: Packet, custom: * ):void {
			mapComm.hideLoading();
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
			profileData.victoryPoint = packet.readFloat();
			profileData.created = packet.readUInt();			
			
			profileData.resources = new Resources(packet.readUInt(), packet.readUInt(), packet.readUInt(), packet.readUInt(), 0);
			
			profileData.members = [];
			var memberCount: int = packet.readShort();
			for (var i: int = 0; i < memberCount; i++) {
				var memberData: * = new Object();
				
				memberData.playerId = packet.readUInt();
				memberData.playerName = packet.readString();
				memberData.cityCount = packet.readInt();
				memberData.rank = packet.readUByte();
				var lastLogin: uint = packet.readUInt();
				if (lastLogin == 0) {
					memberData.date = "Now";
				} else {
					memberData.date = Util.simpleTime(Global.map.getServerTime() - lastLogin) + " ago";
				}
				memberData.contribution = new Resources(packet.readUInt(), packet.readUInt(), packet.readUInt(), packet.readUInt(), 0);

				profileData.members.push(memberData);
			
			}
				
			(profileData.members as Array).sortOn("rank", [Array.NUMERIC]);
			
			profileData.incomingAttacks = [];
			var incomingAttackCount: int = packet.readShort();
			for (i = 0; i < incomingAttackCount; i++) {
				profileData.incomingAttacks.push( {
					target: mapComm.General.readLocation(packet),
					source: mapComm.General.readLocation(packet),
					endTime: packet.readUInt()
				});
			}
			
			profileData.assignments = [];
			var assignmentCount: int = packet.readShort();
			for (i = 0; i < assignmentCount; i++) {
				var assignment: * = {};
				assignment.id = packet.readInt();
				assignment.endTime = packet.readUInt();
				assignment.x = packet.readUInt();
				assignment.y = packet.readUInt();
				assignment.target = mapComm.General.readLocation(packet);
				assignment.attackMode = packet.readByte();
				assignment.dispatchCount = packet.readUInt();
				assignment.description = packet.readString();
				assignment.isAttack = packet.readByte();
				assignment.troopCount = packet.readInt();
				assignment.troops = [];

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
			
			// Strongholds
			var stronghold: *;
			
			profileData.strongholds = [];
			var strongholdCount: int = packet.readShort();
			for (i = 0; i < strongholdCount; i++) {
				stronghold = {
					id: packet.readUInt(),
					name: packet.readString(),
					state: packet.readByte(),
					lvl: packet.readByte(),
					gate: packet.readFloat(),
					x: packet.readUInt(),
					y: packet.readUInt(),
					upkeep: packet.readInt(),
					victoryPointRate: packet.readFloat(),
					dateOccupied: packet.readUInt(),
					gateOpenTo: {
						id: packet.readUInt(),
						name: packet.readString()
					},
					battleState: packet.readByte()
				};
				
				if (stronghold.battleState != Stronghold.BATTLE_STATE_NONE) {
					stronghold.battleId = packet.readUInt();
				}

				if (!Global.map.usernames.strongholds.get(stronghold.id)) {
					Global.map.usernames.strongholds.add(new Username(stronghold.id, stronghold.name));
				}
				
				profileData.strongholds.push(stronghold);
			}
			
			// Attackable strongholds
			profileData.openStrongholds = [];
			strongholdCount  = packet.readShort();
			for (i = 0; i < strongholdCount; i++) {
				stronghold = {
					id: packet.readUInt(),
					name: packet.readString(),
					tribeId: packet.readUInt(),
					tribeName: packet.readString(),					
					state: packet.readByte(),
					lvl: packet.readByte(),
					x: packet.readUInt(),
					y: packet.readUInt(),
					battleState: packet.readByte()
				};
				
				if (stronghold.battleState != Stronghold.BATTLE_STATE_NONE) {
					stronghold.battleId = packet.readUInt();
				}

				if (!Global.map.usernames.strongholds.get(stronghold.id)) {
					Global.map.usernames.strongholds.add(new Username(stronghold.id, stronghold.name));
				}
				
				profileData.openStrongholds.push(stronghold);
			}			
			
			if (custom.callback) {
				custom.callback(profileData);
			}
			else
			{			
				if (!profileData) 
					return;
			
				var dialog: TribeProfileDialog = new TribeProfileDialog(profileData);
				dialog.show();		
			}		
		}
		
		public function readPublicTribeProfile(packet: Packet, custom: * ):void {
			mapComm.hideLoading();
			if (MapComm.tryShowError(packet)) {
				custom.callback(null);
				return;
			}
			
			var profileData: * = new Object();
			profileData.tribeId = packet.readUInt();
			profileData.tribeName = packet.readString();
			profileData.level = packet.readUByte();
			profileData.created = packet.readUInt();
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
			
			if (custom.callback)
				custom.callback(profileData);
			else
			{			
				if (!profileData) 
					return;
			
				var dialog: TribePublicProfileDialog = new TribePublicProfileDialog(profileData);
				dialog.show();		
			}			
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
		
		public function transfer(playerName: String): void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.TRIBE_TRANSFER;
			packet.writeString(playerName);
			
			session.write(packet, showErrorOrRefreshTribePanel, { refresh: true } );
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
			
			session.write(packet, showErrorOrRefreshTribePanel, { message: { title: "Tribe", content: "You have left the tribe" }, close: true } );
			BuiltInMessages.hideTribeAssignmentIncoming();
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
		
		public function onReceiveNotifications(packet: Packet, custom: * ):void {
			var incoming:int = packet.readInt();
			var assignment:int = packet.readShort();
			BuiltInMessages.showTribeAssignmentIncoming(assignment, incoming);
		}
		
		public function onTribeJoin(assignment:int, incoming:int):void {
			BuiltInMessages.showTribeAssignmentIncoming(assignment, incoming);
		}
		public function onTribeLeave():void {
			BuiltInMessages.hideTribeAssignmentIncoming();
		}
	}

}


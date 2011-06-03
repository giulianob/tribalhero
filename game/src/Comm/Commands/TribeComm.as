package src.Comm.Commands {

	import src.Comm.*;
	import src.Constants;
	import src.Map.*;
	import src.Objects.*;
	import src.Objects.Actions.*;
	import src.Objects.Troop.*;
	import src.Global;
	import src.UI.Dialog.InfoDialog;
	import src.UI.Dialog.TribeProfileDialog;

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

		public function invitationConfirm(response: Boolean) : void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.TRIBESMAN_CONFIRM;
			packet.writeByte(response ? 1 : 0);

			session.write(packet, showErrorOrRefreshTribePanel);
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
			
			session.write(packet, showErrorOrRefreshTribePanel, { message: { title: "Invitation sent", content: "An invitation has been sent to this player to join your tribe." }, refresh: false });
		}
		
		public function dismantle() : void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.TRIBE_DELETE;
			
			session.write(packet, showErrorOrRefreshTribePanel, { message: { title: "Tribe dismantled", content: "Your tribe has been dismantled" }, close: true });
		}	

		public function leave() : void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.TRIBESMAN_LEAVE;
			
			session.write(packet, showErrorOrRefreshTribePanel, { message: { title: "Tribe", content: "You have left the tribe" }, close: true });
		}			
		
		public function showErrorOrRefreshTribePanel(packet: Packet, custom: *): void {
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
	}

}


package src.Comm.Commands {

	import src.Comm.*;
	import src.Constants;
	import src.Map.*;
	import src.Objects.*;
	import src.Objects.Actions.*;
	import src.Objects.Troop.*;
	import src.Global;

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

			session.write(packet, mapComm.catchAllErrors);
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

			session.write(packet, mapComm.catchAllErrors);
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
			var memberCount: int = packet.readInt();
			for (var i: int = 0; i < memberCount; i++)
				profileData.members.push({
					playerId: packet.readUInt(),
					playerName: packet.readString(),
					cityCount: packet.readInt(),
					rank: packet.readUByte(),
					contribution: new Resources(packet.readUInt(), packet.readUInt(), packet.readUInt(), packet.readUInt(), 0)
				});
				
			(profileData.members as Array).sortOn("rank", [Array.NUMERIC]);
			custom.callback(profileData);
		}

	}

}


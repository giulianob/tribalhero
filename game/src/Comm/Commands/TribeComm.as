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
	}

}


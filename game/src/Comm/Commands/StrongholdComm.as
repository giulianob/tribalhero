package src.Comm.Commands 
{
	import src.Comm.Packet;
	import src.Comm.Commands;
	import src.Comm.Session;
	import src.Map.MapComm;
	import src.Objects.Stronghold.Stronghold;
	/**
	 * ...
	 * @author Anthony Lam
	 */
	public class StrongholdComm 
	{
		private var mapComm: MapComm;
		private var session: Session;
		
		public function StrongholdComm(mapComm: MapComm) 
		{
			this.mapComm = mapComm;
			this.session = mapComm.session;
		}
		
		public function dispose() : void {
		}
		
		public function viewStrongholdProfile(id: int): void
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.STRONGHOLD_INFO;

			packet.writeUInt(id);

			session.write(packet, function(packet: Packet, custom: * ): void {
				if (MapComm.tryShowError(packet)) return;
				trace("private id:" + packet.readUInt().toString());
				trace("private gate:" + packet.readInt().toString());
				trace("private state:" + packet.readByte().toString());
				trace("private troop count:" + packet.readByte().toString());
				
			}, null);
		}
	
		public function viewStrongholdPublicProfile(id: int): void
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.STRONGHOLD_PUBLIC_INFO;
			packet.writeUInt(id);

			session.write(packet, function(packet: Packet, custom: * ): void {
				if (MapComm.tryShowError(packet)) return;
				trace("public id:" + packet.readUInt().toString());
				trace("public state:" + packet.readByte().toString());
				trace("public occupied:" + packet.readByte().toString());
				trace("public tribe:" + packet.readUInt().toString());
			}, null);
		}
	}

}
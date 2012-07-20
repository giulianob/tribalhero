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
		
		public function getStrongholdInfo(obj: Stronghold):void
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.TROOP_INFO;

			packet.writeUInt(obj.objectId);

			session.write(packet, function(packet: Packet, custom: * ): void {
				if (MapComm.tryShowError(packet)) return;
				var stronghold: Stronghold = custom as Stronghold;
			}, obj);
		}
	}

}
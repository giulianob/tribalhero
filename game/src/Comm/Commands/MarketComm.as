package src.Comm.Commands {
    import src.Comm.*;
    import src.Map.*;

    public class MarketComm {
		
		private var mapComm: MapComm;		
		private var session: Session;
		
		public function MarketComm(mapComm: MapComm) {
			this.mapComm = mapComm;			
			this.session = mapComm.session;
		}
		
		public function dispose() : void {
			
		}		
		
		public function getResourcePrices(callback: Function):void
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.MARKET_PRICES;
			session.write(packet, onReceiveResourcePrices, callback);
		}
		
		public function onReceiveResourcePrices(packet: Packet, custom: * ):void
		{						
			var crop: int = packet.readUShort();
			var wood: int = packet.readUShort();
			var iron: int = packet.readUShort();
			
			custom(wood, iron, crop);
		}
		
		public function buyResources(callback: Function, custom: *, cityId: int, objectId: int, resourceType: int, amount: int, originalPrice: int):void
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.MARKET_BUY;
			
			packet.writeUInt(cityId);
			packet.writeUInt(objectId);
			packet.writeUByte(resourceType);
			packet.writeUShort(amount);
			packet.writeUShort(originalPrice);
			
			session.write(packet, onReceiveBuyResources, [callback, custom]);
		}
		
		public function sellResources(callback: Function, custom: *, cityId: int, objectId: int, resourceType: int, amount: int, originalPrice: int):void
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.MARKET_SELL;
			
			packet.writeUInt(cityId);
			packet.writeUInt(objectId);
			packet.writeUByte(resourceType);
			packet.writeUShort(amount);
			packet.writeUShort(originalPrice);
			
			session.write(packet, onReceiveBuyResources, [callback, custom]);
		}		
		
		public function onReceiveBuyResources(packet: Packet, custom: * ) :void
		{
			var status: int = 0;
			if ((packet.option & Packet.OPTIONS_FAILED) == Packet.OPTIONS_FAILED)
				status = packet.readUInt();			
			
			custom[0](status, custom[1]);		
		}
	}
	
}
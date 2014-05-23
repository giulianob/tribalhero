package src.Comm {
    import flash.events.Event;

    public class PacketEvent extends Event {
		
		public var packet: Packet;
		
		public function PacketEvent(type: String, packet: Packet) {
			super(type);
			this.packet = packet;
		}
	}
}
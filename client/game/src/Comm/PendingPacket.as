/**
* ...
* @author Default
* @version 0.1
*/

package src.Comm {
    import com.codecatalyst.promise.Deferred;

    internal class PendingPacket
	{
		public var seq: int;
		public var callback: Function;
		public var custom: *;
        public var deferred: Deferred;
		
		public function PendingPacket(seq: int, callback: Function, custom: *, deferred: Deferred)
		{
			this.seq = seq;
			this.callback = callback;
			this.custom = custom;
            this.deferred = deferred;
        }

        public function resolve(packet: Packet): void {
            if (packet.hasError()) {
                deferred.reject(packet);
            }
            else {
                deferred.resolve(packet);
            }
        }
	}
}
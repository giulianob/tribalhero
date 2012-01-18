/**
* ...
* @author Default
* @version 0.1
*/

package src.Comm {
	internal class PendingPacket
	{
		public var seq: int;
		public var callback: Function;
		public var custom: *;
		
		public function PendingPacket(seq: int, callback: Function, custom: *) 
		{
			this.seq = seq;
			this.callback = callback;
			this.custom = custom;
		}
	}
}

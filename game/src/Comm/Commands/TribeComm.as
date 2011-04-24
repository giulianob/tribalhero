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

			}
		}


	}

}


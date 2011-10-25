package src.Comm.Commands
{
	import src.Comm.*;
	import src.Map.MapComm;

	public class MessagingComm
	{
		private var mapComm: MapComm;
		private var session: Session;

		public function MessagingComm(mapComm: MapComm) {
			this.mapComm = mapComm;
			this.session = mapComm.session;
		}
		
		public function dispose() : void {
			
		}

		public function list(loader: GameURLLoader, folder: String, page: int) : void {
			loader.load("/messages/listing", [ { key: "folder", value: folder }, { key: "page", value: page } ]);
		}
		
		public function del(loader: GameURLLoader, ids: Array) : void {
			loader.load("/messages/del", [ { key: "ids", value: ids } ]);
		}
		
		public function markAsRead(loader: GameURLLoader, ids: Array) : void {
			loader.load("/messages/mark_as_read", [ { key: "ids", value: ids } ]);
		}		
		
		public function view(loader: GameURLLoader, id: int) : void {
			loader.load("/messages/view", [ { key: "id", value: id } ]);
		}		
		
		public function send(loader: GameURLLoader, to: String, subject: String, message: String) : void {
			loader.load("/messages/send", [ { key: "to", value: to }, { key: "subject", value: subject }, { key: "message", value: message } ]);
		}		
	}

}


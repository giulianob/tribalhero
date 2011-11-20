package src.Comm.Commands 
{
	import src.Comm.*;
	import src.Map.Map;
	import src.Map.MapComm;
	
	public class BattleReportComm
	{
		private var mapComm: MapComm;		
		private var session: Session;
			
		public function BattleReportComm(mapComm: MapComm) {
			this.mapComm = mapComm;			
			this.session = mapComm.session;
		}
		
		public function dispose() : void {
		
		}
		
		public function listLocal(loader: GameURLLoader, page: int, playerNameFilter: String) : void {
			loader.load("/reports/index_local", [ { key: "page", value: page }, { key: "playerNameFilter", value: playerNameFilter } ]);
		}
		
		public function markAllAsRead(loader: GameURLLoader) : void {
			loader.load("/reports/mark_all_as_read", []);
		}		
		
		public function viewLocal(loader: GameURLLoader, id: int, playerNameFilter: String) : void {
			loader.load("/reports/view_local", [ { key: "id", value: id }, { key: "playerNameFilter", value: playerNameFilter } ]);
		}
		
		public function listRemote(loader: GameURLLoader, page: int, playerNameFilter: String) : void {
			loader.load("/reports/index_remote", [ { key: "page", value: page }, { key: "playerNameFilter", value: playerNameFilter }]);
		}
		
		public function viewRemote(loader: GameURLLoader, id: int, playerNameFilter: String) : void {
			loader.load("/reports/view_remote", [ { key: "id", value: id }, { key: "playerNameFilter", value: playerNameFilter } ]);
		}		
	}

}
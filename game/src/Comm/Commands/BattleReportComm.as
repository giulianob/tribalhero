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
		
		public function listLocal(loader: GameURLLoader, viewType: int, page: int, playerNameFilter: String) : void {
			loader.load("/reports/index_local", [ { key: "viewType", value: viewType }, { key: "page", value: page }, { key: "playerNameFilter", value: playerNameFilter } ]);
		}
		
		public function markAllAsRead(loader: GameURLLoader) : void {
			loader.load("/reports/mark_all_as_read", []);
		}		
		
		public function viewReport(loader: GameURLLoader, id: int, playerNameFilter: String, viewType: int) : void {
			loader.load("/reports/view_report", [ { key: "id", value: id }, { key: "playerNameFilter", value: playerNameFilter }, { key: "viewType", value: viewType } ]);
		}
		
		public function viewMoreEvents(loader: GameURLLoader, id: int, playerNameFilter: String, viewType: int, page: int) : void {
			loader.load("/reports/view_more_events", [ { key: "id", value: id }, { key: "playerNameFilter", value: playerNameFilter }, { key: "viewType", value: viewType }, { key: "page", value: page } ]);
		}		
		
		public function viewSnapshot(loader: GameURLLoader, id: int, playerNameFilter: String, viewType: int, reportId: int) : void {
			loader.load("/reports/view_snapshot", [ { key: "id", value: id }, { key: "playerNameFilter", value: playerNameFilter }, { key: "viewType", value: viewType }, { key: "reportId", value: reportId } ]);
		}				
		
		public function listRemote(loader: GameURLLoader, viewType: int, page: int, playerNameFilter: String) : void {
			loader.load("/reports/index_remote", [ { key: "viewType", value: viewType }, { key: "page", value: page }, { key: "playerNameFilter", value: playerNameFilter }]);
		}		
	}

}
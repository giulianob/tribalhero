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
		
		public function listLocal(loader: GameURLLoader, page: int) : void {
			loader.load("/reports/index_local", [ { key: "page", value: page } ]);
		}
		
		public function viewLocal(loader: GameURLLoader, id: int) : void {
			loader.load("/reports/view_local", [ { key: "id", value: id }]);
		}
		
		public function listRemote(loader: GameURLLoader, page: int) : void {
			loader.load("/reports/index_remote", [ { key: "page", value: page } ]);
		}
		
		public function viewRemote(loader: GameURLLoader, id: int) : void {
			loader.load("/reports/view_remote", [ { key: "id", value: id }]);
		}		
	}

}
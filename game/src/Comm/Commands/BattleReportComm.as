package src.Comm.Commands 
{
	import src.Comm.*;
	import src.Map.Map;
	import src.Map.MapComm;
	
	public class BattleReportComm
	{
		private var mapComm: MapComm;
		private var map: Map;
		private var session: Session;
			
		public function BattleReportComm(mapComm: MapComm) {
			this.mapComm = mapComm;
			this.map = mapComm.map;
			this.session = mapComm.session;
		}
		
		public function list(loader: GameURLLoader, page: int) : void {
			loader.load("/reports/index", [ { key: "page", value: page } ]);
		}
		
		public function view(loader: GameURLLoader, id: int, page: int) : void {
			loader.load("/reports/view", [ { key: "id", value: id }, { key: "page", value: page } ]);
		}
	}

}
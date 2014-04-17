package src.Comm.Commands
{
    import src.Comm.*;
    import src.Map.MapComm;

    public class RankingComm
	{
		private var mapComm: MapComm;
		private var session: Session;

		public function RankingComm(mapComm: MapComm) {
			this.mapComm = mapComm;
			this.session = mapComm.session;
		}
		
		public function dispose() : void {
			
		}

		public function list(loader: GameURLLoader, id: int, type: int, page: int) : void {
			loader.load("/rankings/listing", [ { key: "id", value: id }, { key: "type", value: type }, { key: "page", value: page } ]);
		}

		public function search(loader: GameURLLoader, search: String, type: int) : void {
			loader.load("/rankings/search", [ { key: "search", value: search }, { key: "type", value: type } ]);
		}
	}

}

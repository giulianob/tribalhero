﻿package src.Map {

	import src.Comm.*;
	import src.Comm.Commands.*;
	import src.Objects.*;
	import src.UI.Dialog.*;

	public class MapComm {

		public var Battle: BattleComm;
		public var City: CityComm;
		public var General: GeneralComm;
		public var Objects: ObjectComm;
		public var Region: RegionComm;
		public var Troop: TroopComm;
		public var Market: MarketComm;
		public var BattleReport: BattleReportComm;
		public var Ranking: RankingComm;
		public var Messaging: MessagingComm;
		public var Tribe: TribeComm;
		public var MessageBoard: MessageBoardComm;

		public var session: Session;
		
		private var pnlLoading: InfoDialog;

		public function MapComm(session: Session)
		{
			this.session = session;
			Battle = new BattleComm(this);
			City = new CityComm(this);
			General = new GeneralComm(this);
			Objects = new ObjectComm(this);
			Region = new RegionComm(this);
			Troop = new TroopComm(this);
			Market = new MarketComm(this);
			BattleReport = new BattleReportComm(this);
			Ranking = new RankingComm(this);
			Messaging = new MessagingComm(this);
			Tribe = new TribeComm(this);
			MessageBoard = new src.Comm.Commands.MessageBoardComm(this);
		}

		public function dispose() : void {
			if (this.session) {
				Battle.dispose();
				City.dispose();
				General.dispose();
				Objects.dispose();
				Region.dispose();
				Troop.dispose();
				Market.dispose();
				BattleReport.dispose();
				Ranking.dispose();
				Messaging.dispose();
			}
		}
		
		public static function tryShowError(packet: Packet, callback: Function = null, showDirectlyToStage: Boolean = false, ignoreErrors: Array = null) : Boolean {
			if ((packet.option & Packet.OPTIONS_FAILED) == Packet.OPTIONS_FAILED)
			{
				var err: int = packet.readUInt();
				
				if (ignoreErrors && ignoreErrors.indexOf(err) >= 0)
					return true;

				GameError.showMessage(err, callback, showDirectlyToStage);
				return true;
			}

			return false;
		}

		public function catchAllErrors(packet: Packet, custom: *):void
		{
			hideLoading();
			
			var hasError: Boolean = (packet.option & Packet.OPTIONS_FAILED) == Packet.OPTIONS_FAILED;
			
			if (hasError)
			{
				var err: int = packet.readUInt();

				GameError.showMessage(err);
			}
			
			if (custom != null) {
				if (custom is Function)
					custom();			
				else if (custom is Object && !hasError && (custom as Object).hasOwnProperty("message"))
					InfoDialog.showMessageDialog(custom.message.title, custom.message.content);
			}
		}
		
		public function showLoading(message: String = "Loading...", title: String = "Tribal Hero"): void {
			hideLoading();
			pnlLoading = InfoDialog.showMessageDialog(title, message, null, null, true, false, 0);
		}
		
		public function hideLoading():void {
			if (!pnlLoading) 
				return;
				
			pnlLoading.getFrame().dispose();
			pnlLoading = null;
		}
	}
}


﻿package src.Map {

	import src.Comm.*;
	import src.Objects.GameError;
	import src.UI.Dialog.*;
	import src.Comm.Commands.*;

	public class MapComm {

		public var Battle: BattleComm;
		public var City: CityComm;
		public var Login: LoginComm;
		public var Object: ObjectComm;
		public var Region: RegionComm;
		public var Troop: TroopComm;
		public var Market: MarketComm;
		public var BattleReport: BattleReportComm;
		
		public var session: Session;		

		public function MapComm(session: Session)
		{
			this.session = session;
			Battle = new BattleComm(this);
			City = new CityComm(this);
			Login = new LoginComm(this);
			Object = new ObjectComm(this);
			Region = new RegionComm(this);
			Troop = new TroopComm(this);
			Market = new MarketComm(this);
			BattleReport = new BattleReportComm(this);
		}
		
		public function tryShowError(packet: Packet) : Boolean {
			if ((packet.option & Packet.OPTIONS_FAILED) == Packet.OPTIONS_FAILED)
			{
				var err: int = packet.readUInt();

				GameError.showMessage(err);
				return true;
			}

			return false;
		}

		public function catchAllErrors(packet: Packet, custom: * ):void
		{
			if ((packet.option & Packet.OPTIONS_FAILED) == Packet.OPTIONS_FAILED)
			{
				var err: int = packet.readUInt();

				GameError.showMessage(err);
			}
		}
	}
}

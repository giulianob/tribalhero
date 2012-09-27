package src.Comm.Commands 
{
	import src.Comm.Packet;
	import src.Comm.Commands;
	import src.Comm.Session;
	import src.Map.MapComm;
	import src.Objects.SimpleGameObject;
	import src.Objects.Stronghold.Stronghold;
	import src.UI.Dialog.StrongholdProfileDialog;
	import src.Objects.Troop.*;
	import src.Global;
	import src.Map.Username;
	/**
	 * ...
	 * @author Anthony Lam
	 */
	public class StrongholdComm 
	{
		private var mapComm: MapComm;
		private var session: Session;
		
		public function StrongholdComm(mapComm: MapComm) 
		{
			this.mapComm = mapComm;
			this.session = mapComm.session;
		}
		
		public function dispose() : void {
		}
		
		public function viewStrongholdProfile(id: int): void
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.STRONGHOLD_INFO;

			packet.writeUInt(id);

			session.write(packet, onReceiveStrongholdProfile, null);
		}
		
		private function onReceiveStrongholdProfile(packet: Packet, custom: * ): void {
			var profileData: * = new Object();
			if (MapComm.tryShowError(packet)) return;
			profileData.strongholdId = packet.readUInt();
			profileData.strongholdName = packet.readString();
			profileData.strongholdLevel = packet.readByte();
			profileData.strongholdGate = packet.readInt();
			profileData.strongholdVictoryPointRate = packet.readFloat();
			profileData.strongholdDateOccupied = packet.readUInt();
			profileData.strongholdX = packet.readUInt();
			profileData.strongholdY = packet.readUInt();
			profileData.strongholdObjectState = packet.readUByte();
		
			if(profileData.strongholdObjectState==SimpleGameObject.STATE_BATTLE) {
				profileData.strongholdBattleId = packet.readUInt();
			}

			profileData.troops = [];
			var troopCount: int = packet.readByte();
			for (var i: int = 0; i < troopCount; i++) {
				var troop: * = {
					playerId: packet.readUInt(),
					cityId: packet.readUInt(),
					playerName: packet.readString(),
					cityName: packet.readString(),
					stub: null
				};
				
				troop.stub = new TroopStub(packet.readByte(), troop.playerId, troop.cityId);
				
				Global.map.usernames.players.add(new Username(troop.playerId, troop.playerName));
				Global.map.usernames.cities.add(new Username(troop.cityId, troop.cityName));
				
				var stub: TroopStub = troop.stub;
				
				var formationCnt: int = packet.readByte();
				for (var formationIter: int = 0; formationIter < formationCnt; formationIter++) {
					var formation: Formation = new Formation(packet.readByte());
					
					var unitCount: int = packet.readByte();
					for (var unitIter: int = 0; unitIter < unitCount; unitIter++) {
						formation.add(new Unit(packet.readUShort(), packet.readUShort()));
					}
					
					stub.add(formation);
				}
				
				profileData.troops.push(troop);
			}
			
			if (custom && custom.callback)
				custom.callback(profileData);
			else
			{
				if (!profileData) 
					return;
				var dialog: StrongholdProfileDialog = new StrongholdProfileDialog(profileData);
				dialog.show();		
			}
		}
		
		public function viewStrongholdPublicProfile(id: int): void
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.STRONGHOLD_PUBLIC_INFO;
			packet.writeUInt(id);

			session.write(packet, onReceiveStrongholdPublicProfile, null);
		}
						
		private function onReceiveStrongholdPublicProfile(packet: Packet, custom: * ): void {
			if (MapComm.tryShowError(packet)) return;
			trace("public id:" + packet.readUInt().toString());
			trace("public state:" + packet.readByte().toString());
			trace("public occupied:" + packet.readByte().toString());
			trace("public tribe:" + packet.readUInt().toString());
		}
		
		public function viewStrongholdProfileByName(name: String , callback: Function = null):void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.STRONGHOLD_PUBLIC_INFO_BY_NAME;
			packet.writeString(name);
			session.write(packet, onReceiveStrongholdProfileByName, {callback: callback});
		}

		public function onReceiveStrongholdProfileByName(packet: Packet, custom: * ): void {
			if (MapComm.tryShowError(packet)) {
				if(custom.callback!=null) custom.callback(null);
				return;
			}
			
			var isPrivate: Boolean = packet.readByte()==1;
			if (isPrivate) {
				onReceiveStrongholdProfile(packet, custom);
			} else {
				onReceiveStrongholdPublicProfile(packet, custom);
			}
		}

	}

}
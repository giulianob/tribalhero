package src.Comm.Commands 
{
	import fl.lang.Locale;
	import flash.geom.Point;
	import org.aswing.AsWingConstants;
	import src.Comm.Packet;
	import src.Comm.Commands;
	import src.Comm.Session;
	import src.Map.MapComm;
	import src.Map.MapUtil;
	import src.Objects.SimpleGameObject;
	import src.Objects.Stronghold.Stronghold;
	import src.UI.Dialog.InfoDialog;
	import src.UI.Dialog.StrongholdProfileDialog;
	import src.Objects.Troop.*;
	import src.Global;
	import src.Map.Username;
	import src.Util.Util;

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
		
		public function viewStrongholdProfile(id: int, custom: * = null): void
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.STRONGHOLD_INFO;

			packet.writeUInt(id);

			session.write(packet, onReceiveStrongholdProfile, custom);
			mapComm.showLoading();
		}
		
		private function readStrongholdPublicProfile(packet: Packet, custom: * ): void {
			// We don't actually have a public profile we just send the map there
			var pt:Point = MapUtil.getScreenCoord(packet.readUInt(), packet.readUInt());
			Global.map.camera.ScrollToCenter(pt.x, pt.y);
			Global.gameContainer.closeAllFrames(true);
			
			if (custom && custom.callback) {
				custom.callback();
			}
		}
		
		private function readStrongholdPrivateProfile(packet: Packet, custom: * ): void {
			var profileData: * = {};
			profileData.strongholdId = packet.readUInt();
			profileData.strongholdName = packet.readString();
			profileData.strongholdLevel = packet.readByte();
			profileData.strongholdGate = packet.readFloat();
            profileData.strongholdGateMax = packet.readInt();
			profileData.strongholdVictoryPointRate = packet.readFloat();
			profileData.strongholdDateOccupied = packet.readUInt();
			profileData.strongholdX = packet.readUInt();
			profileData.strongholdY = packet.readUInt();
			profileData.strongholdObjectState = packet.readUByte();
		
			if(profileData.strongholdObjectState==SimpleGameObject.STATE_BATTLE) {
				profileData.strongholdBattleId = packet.readUInt();
			}
			
			profileData.troops = [];
			var troopCount: int = packet.readUShort();
			for (var i: int = 0; i < troopCount; i++) {
				var troop: * = {
					playerId: packet.readUInt(),
					cityId: packet.readUInt(),
					playerName: packet.readString(),
					cityName: packet.readString(),
					stub: null
				};
				
				troop.stub = new TroopStub(packet.readUShort(), troop.playerId, troop.cityId);
				
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
		
		public function onReceiveStrongholdProfile(packet: Packet, custom: * ): void {
			mapComm.hideLoading();
			if (MapComm.tryShowError(packet)) return;

			var isPrivate: Boolean = packet.readByte()==1;
			if (isPrivate) {
				readStrongholdPrivateProfile(packet, custom);
			} else {
				readStrongholdPublicProfile(packet, custom);
			}			
		}
		
		public function viewStrongholdProfileByName(name: String , callback: Function = null):void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.STRONGHOLD_PUBLIC_INFO_BY_NAME;
			packet.writeString(name);
			session.write(packet, onReceiveStrongholdProfile, { callback: callback } );
			mapComm.showLoading();
		}
		
		public function gotoStrongholdLocation(strongholdId:int):void
		{
			var packet:Packet = new Packet();
			packet.cmd = Commands.STRONGHOLD_LOCATE;
			packet.writeUInt(strongholdId);
			
			session.write(packet, onReceiveStrongholdLocation);
		}
		
		public function gotoStrongholdLocationByName(name:String):void
		{
			var packet:Packet = new Packet();
			packet.cmd = Commands.STRONGHOLD_LOCATE;
			packet.writeUInt(0);
			packet.writeString(name);
			
			session.write(packet, onReceiveStrongholdLocation);
		}		
		
		public function onReceiveStrongholdLocation(packet:Packet, custom:*):void
		{
			if (MapComm.tryShowError(packet)) {
				return;
			}
			var pt:Point = MapUtil.getScreenCoord(packet.readUInt(), packet.readUInt());
			Global.map.camera.ScrollToCenter(pt.x, pt.y);
			Global.gameContainer.closeAllFrames(true);
		}		
		
		public function repairStrongholdGate(id: uint, callback: Function = null): void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.STRONGHOLD_GATE_REPAIR;
			packet.writeUInt(id);
			session.write(packet, onRepairStrongholdGate, { callback: callback });
		}
		
		public function onRepairStrongholdGate(packet: Packet, custom: *): void {
			if (!MapComm.tryShowError(packet)) {
				InfoDialog.showMessageDialog("Info",Locale.loadString("STRONGHOLD_GATE_REPAIRED"));
			}
			
			if (custom.callback) {
				custom.callback();
			}
		}
		
		public function onListStrongholds(packet: Packet, custom: *): void {
			if (!MapComm.tryShowError(packet)) {
							// Strongholds
				var stronghold: *;
				var strongholds: * = [];
				
				var strongholdCount: int = packet.readShort();
				for (var i:int = 0; i < strongholdCount; i++) {
					stronghold = {
						id: packet.readUInt(),
						name: packet.readString(),
						lvl: packet.readByte(),
						x: packet.readUInt(),
						y: packet.readUInt()
					};
					
					if (!Global.map.usernames.strongholds.get(stronghold.id)) {
						Global.map.usernames.strongholds.add(new Username(stronghold.id, stronghold.name));
					}
					
					strongholds.push(stronghold);
				}
			
				if (custom.callback) {
					custom.callback(strongholds);
				}
			}
		}
		
		public function listStrongholds(callback: Function = null): void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.STRONGHOLD_LIST;
			session.write(packet, onListStrongholds, { callback: callback });
		}
	}
}

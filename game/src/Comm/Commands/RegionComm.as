package src.Comm.Commands {

	import src.Comm.*;
	import src.Map.*;
	import src.Objects.*;
	import src.Constants;
	import src.Global;
	import src.Objects.Factories.ObjectFactory;
	import src.Objects.States.BattleState;
	import src.Objects.States.GameObjectState;

	public class RegionComm {

		private var mapComm: MapComm;
		private var session: Session;

		public function RegionComm(mapComm: MapComm) {
			this.mapComm = mapComm;
			this.session = mapComm.session;

			session.addEventListener(Commands.CHANNEL_NOTIFICATION, onChannelReceive);
		}
		
		public function dispose() : void {
			session.removeEventListener(Commands.CHANNEL_NOTIFICATION, onChannelReceive);
		}

		public function onChannelReceive(e: PacketEvent):void
		{
			switch(e.packet.cmd)
			{
				case Commands.REGION_SET_TILE:
					onRegionSetTile(e.packet);
				break;
			}
		}

		public function buildRoad(cityId: int, x: int, y: int) : void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.REGION_ROAD_BUILD;
			packet.writeUInt(cityId);
			packet.writeUInt(x);
			packet.writeUInt(y);
			session.write(packet, mapComm.catchAllErrors);
		}

		public function destroyRoad(cityId: int, x: int, y: int) : void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.REGION_ROAD_DESTROY;
			packet.writeUInt(cityId);
			packet.writeUInt(x);
			packet.writeUInt(y);
			session.write(packet, mapComm.catchAllErrors);
		}

		public function onRegionSetTile(packet: Packet):void {
			var cnt: int = packet.readUShort();
			var regionId: int;

			for (var i: int = 0; i < cnt; i++) {
				var x: int = packet.readUInt();
				var y: int = packet.readUInt();
				var tileType: int = packet.readUShort();

				Global.map.regions.setTileType(x, y, tileType, false);

				regionId = MapUtil.getRegionIdFromMapCoord(x, y);
			}

			Global.map.regions.redrawRegion(regionId);
		}

		public function getRegion(ids: Array, outdatedIds: Array):void
		{
			var packet:Packet = new Packet();
			packet.cmd = Commands.REGION_GET;
			packet.option = 0;

			packet.writeUByte(ids.length);
			for (var i:int = 0; i < ids.length; i++)
			{
				packet.writeUShort(ids[i]);
			}

			packet.writeUByte(outdatedIds.length);
			for (i = 0; i < outdatedIds.length; i++)
			{
				packet.writeUShort(outdatedIds[i]);
			}

			session.write(packet, onReceiveRegion);
		}

		public function onReceiveRegion(packet:Packet, custom: *):void
		{
			var regionCnt: int = packet.readUByte();
			for (var i:int = 0; i < regionCnt; i++)
			{
				var id: int = packet.readUShort();
				var mapArray:Array = packet.read2dShortArray(Constants.regionTileW, Constants.regionTileH);

				var newRegion: Region = Global.map.addRegion(id, mapArray);

				var objCnt: int = packet.readUShort();

				for (var j: int = 0; j < objCnt; j++)
				{
					var objLvl: int = packet.readUByte();
					var objType: int = packet.readUShort();
					var objPlayerId: int = packet.readUInt();
					var objCityId: int = packet.readUInt();
					var objId: int = packet.readUInt();
					var objHpPercent: int = 100;
					var objX: int = packet.readUShort() + (id % Constants.mapRegionW) * Constants.regionTileW;
					var objY: int = packet.readUShort() + int(id / Constants.mapRegionW) * Constants.regionTileH;
					var objState: int = packet.readUByte();

					var obj:SimpleGameObject = newRegion.addObject(objLvl, objType, objPlayerId, objCityId, objId, objHpPercent, objX, objY);

					mapComm.Object.readState(obj, packet, objState);

					mapComm.Object.readWall(obj, packet);
				}
			}

			Global.map.objContainer.moveWithCamera(Global.gameContainer.camera.x, Global.gameContainer.camera.y);
		}

		public function getCityRegion(ids: Array):void
		{
			var packet:Packet = new Packet();
			packet.cmd = Commands.CITY_REGION_GET;
			packet.option = 0;

			packet.writeUByte(ids.length);
			for (var i:int = 0; i < ids.length; i++)
			{
				packet.writeUShort(ids[i]);
			}

			session.write(packet, onReceiveCityRegion);
		}

		public function onReceiveCityRegion(packet:Packet, custom: *):void
		{
			var regionCnt: int = packet.readUByte();
			for (var i:int = 0; i < regionCnt; i++)
			{
				var id: int = packet.readUShort();

				var newRegion: CityRegion = Global.gameContainer.miniMap.addCityRegion(id);

				var objCnt: int = packet.readUShort();

				for (var j: int = 0; j < objCnt; j++)
				{
					var objType: int = packet.readUByte();
					
					// City objects
					if (objType == 0) {
						var objLvl: int = packet.readUByte();					
						var objPlayerId: int = packet.readUInt();
						var objCityId: int = packet.readUInt();
						var objId: int = 1;						

						var objX: int = packet.readUShort() + (id % Constants.miniMapRegionW) * Constants.cityRegionTileW;
						var objY: int = packet.readUShort() + int(id / Constants.miniMapRegionW) * Constants.cityRegionTileH;

						var obj: SimpleGameObject = newRegion.addCityObject(objLvl, ObjectFactory.getFirstType("MainBuilding"), objPlayerId, objCityId, objId, objX, objY, false);
					}
					// Forest objects
					else if (objType == 1) {
						objLvl = packet.readUByte();
						objId = packet.readUInt();
						
						objX = packet.readUShort() + (id % Constants.miniMapRegionW) * Constants.cityRegionTileW;
						objY = packet.readUShort() + int(id / Constants.miniMapRegionW) * Constants.cityRegionTileH;
						
						obj = newRegion.addForestObject(objLvl, objId, objX, objY, false);
					}
					// Troop objects
					else if (objType == 2) {
						var objTroopId: int = packet.readUByte();
						objPlayerId = packet.readUInt();
						objCityId = packet.readUInt();
						objId = packet.readUInt();
						
						objX = packet.readUShort() + (id % Constants.miniMapRegionW) * Constants.cityRegionTileW;
						objY = packet.readUShort() + int(id / Constants.miniMapRegionW) * Constants.cityRegionTileH;
						
						obj = newRegion.addTroopObject(objPlayerId, objTroopId, objCityId, objId, objX, objY, false);						
					}
				}

				newRegion.sortObjects();
			}

			Global.gameContainer.miniMap.objContainer.moveWithCamera(Global.gameContainer.camera.miniMapX, Global.gameContainer.camera.miniMapY);
		}
	}
}


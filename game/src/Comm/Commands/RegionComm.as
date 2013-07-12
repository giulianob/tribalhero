package src.Comm.Commands {

	import flash.geom.Point;
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
		
		public function createCity(cityId: int, x: int, y: int, cityName: String) : void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.CITY_CREATE;
			packet.writeUInt(cityId);
			packet.writeUInt(x);
			packet.writeUInt(y);
			packet.writeString(cityName);
			session.write(packet, mapComm.catchAllErrors);
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
					var obj: SimpleGameObject = mapComm.Objects.readObjectInstance(packet, newRegion.id, true);
					newRegion.addObject(obj, false);					
				}
				
				newRegion.sortObjects();
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
					var objX: int = packet.readUShort() + (id % Constants.miniMapRegionW) * Constants.cityRegionTileW;
					var objY: int = packet.readUShort() + int(id / Constants.miniMapRegionW) * Constants.cityRegionTileH;
					var objGroupId: int = packet.readUInt();
					var objId: int = packet.readUInt();
					var extraProps : Object = {};
					
					var coord: Point = MapUtil.getMiniMapScreenCoord(objX, objY);
					
					// City objects
					if (objType == ObjectFactory.TYPE_CITY) {
						extraProps.level = packet.readUByte();	
						extraProps.playerId = packet.readUInt();
						extraProps.value = packet.readUShort();
						extraProps.alignment = packet.readFloat();
						extraProps.tribeId = packet.readUInt();
						extraProps.isNewbie = packet.readByte();
					}
					// Forest objects
					else if (objType == ObjectFactory.TYPE_FOREST) {
						extraProps.level = packet.readUByte();
					}
					// Troop objects
					else if (objType == ObjectFactory.TYPE_TROOP_OBJ) {
						extraProps.troopId = packet.readUShort();
						extraProps.tribeId = packet.readUInt();
					}
					// Stronghold objects
					else if (objType == ObjectFactory.TYPE_STRONGHOLD) {
						extraProps.level = packet.readUByte();
						extraProps.tribeId = packet.readUInt();
					}
					else if (objType ==  ObjectFactory.TYPE_BARBARIAN_TRIBE) {
						extraProps.level = packet.readUByte();
						extraProps.count = packet.readUByte();
					}
					newRegion.addRegionObject(objType, objGroupId, objId, coord.x, coord.y, extraProps);
				}
			}

			Global.gameContainer.miniMap.objContainer.moveWithCamera(Global.gameContainer.camera.miniMapX, Global.gameContainer.camera.miniMapY);
		}
	}
}


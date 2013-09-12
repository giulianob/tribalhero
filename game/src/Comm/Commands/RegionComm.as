package src.Comm.Commands {

    import System.Linq.Enumerable;

    import flash.geom.Point;

    import src.Comm.*;
    import src.Constants;
    import src.Global;
    import src.Map.*;
    import src.Map.MiniMap.MiniMapRegion;
    import src.Objects.Factories.ObjectFactory;
    import src.Objects.SimpleGameObject;

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

            var regionIds: Array = [];
			for (var i: int = 0; i < cnt; i++) {
                var pos: Position = new Position(packet.readUInt(), packet.readUInt());
				var tileType: int = packet.readUShort();

				Global.map.regions.setTileType(pos, tileType);

                regionIds.push(TileLocator.getRegionIdFromMapCoord(pos));
			}

            for each (var regionId: int in Enumerable.from(regionIds).distinct().toArray()) {
			    Global.map.regions.redrawRegion(regionId);
            }
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
            var objectsToAdd: Array = [];

			var regionCnt: int = packet.readUByte();
			for (var i:int = 0; i < regionCnt; i++)
			{
				var id: int = packet.readUShort();
                trace("Region " + id);
				var mapArray:Array = packet.read2dShortArray(Constants.regionTileW, Constants.regionTileH);

				var newRegion: Region = Global.map.addRegion(id, mapArray);

				var objCnt: int = packet.readUShort();

				for (var j: int = 0; j < objCnt; j++)
				{
                    objectsToAdd.push(mapComm.Objects.readObjectInstance(packet, newRegion.id, true));
				}
			}

            for each (var obj: SimpleGameObject in objectsToAdd) {
                Global.map.regions.addObject(obj);
            }

			Global.map.objContainer.moveWithCamera(Global.gameContainer.camera.currentPosition.x, Global.gameContainer.camera.currentPosition.y);
		}

		public function getMiniMapRegion(ids: Array):void
		{
			var packet:Packet = new Packet();
			packet.cmd = Commands.MINIMAP_REGION_GET;
			packet.option = 0;

			packet.writeUByte(ids.length);
			for (var i:int = 0; i < ids.length; i++)
			{
				packet.writeUShort(ids[i]);
			}

			session.write(packet, onReceiveMiniMapRegion);
		}

		public function onReceiveMiniMapRegion(packet:Packet, custom: *):void
		{
			var regionCnt: int = packet.readUByte();
			for (var i:int = 0; i < regionCnt; i++)
			{
				var id: int = packet.readUShort();

				var newRegion: MiniMapRegion = Global.gameContainer.miniMap.addMiniMapRegion(id);

				var objCnt: int = packet.readUShort();

				for (var j: int = 0; j < objCnt; j++)
				{
					var objType: int = packet.readUByte();
					var objX: int = packet.readUShort() + (id % Constants.miniMapRegionRatioW) * Constants.miniMapRegionTileW;
					var objY: int = packet.readUShort() + int(id / Constants.miniMapRegionRatioW) * Constants.miniMapRegionTileH;
					var objGroupId: int = packet.readUInt();
					var objId: int = packet.readUInt();
                    var objSize: int = packet.readUByte();
					var extraProps : Object = {};
					
					var position: ScreenPosition = TileLocator.getMiniMapScreenCoord(objX, objY);
					
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
					newRegion.addRegionObject(objType, objGroupId, objId, objSize, position, extraProps);
				}
			}

			Global.gameContainer.miniMap.objContainer.moveWithCamera(Global.gameContainer.camera.miniMapX, Global.gameContainer.camera.miniMapY);
		}
	}
}


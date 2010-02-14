package src.Comm.Commands {
	
	import flash.geom.Point;
	import src.Comm.*;
	import src.Map.*;
	import src.Objects.*;
	import src.Constants;

	public class RegionComm {
		
		private var mapComm: MapComm;
		private var map: Map;
		private var session: Session;
		
		public function RegionComm(mapComm: MapComm) {
			this.mapComm = mapComm;
			this.map = mapComm.map;
			this.session = mapComm.session;			
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
				
				var newRegion: Region = map.addRegion(id, mapArray);
				
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
					
					var obj:SimpleGameObject = newRegion.addObject(objLvl, objType, objPlayerId, objCityId, objId, objHpPercent, objX, objY, false);											
					
					//obj may be null so we have to check it here (but we should still read all the stream)
					if (objState > 0)
					{
						switch(objState)
						{
							case SimpleGameObject.STATE_BATTLE:
								var battleCityId: int = packet.readUInt();
								if (obj) obj.battleCityId = battleCityId;
								break;
							default:
								trace("Unknown object state in onReceiveRegion:" + objState);
								break;
						}
					}
					
					if (obj)
						obj.State = objState;						
					
					if (objId == 1) { // main building
						var radius: int = packet.readUByte();
						if (obj) 
						{
							obj.wall.draw(radius);
						}
					}
				}
				
				newRegion.sortObjects();
			}
			
			map.objContainer.moveWithCamera(map.gameContainer.camera.x, map.gameContainer.camera.y);
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
				
				var newRegion: CityRegion = map.gameContainer.miniMap.addCityRegion(id);
				
				var objCnt: int = packet.readUShort();
				
				for (var j: int = 0; j < objCnt; j++)
				{
					var objLvl: int = packet.readUByte();
					var objType: int = packet.readUShort();
					var objPlayerId: int = packet.readUInt();
					var objCityId: int = packet.readUInt();
					var objId: int = packet.readUInt();
					var objHpPercent: int = 100;
					
					var objX: int = packet.readUShort() + (id % Constants.miniMapRegionW) * Constants.cityRegionTileW;
					var objY: int = packet.readUShort() + int(id / Constants.miniMapRegionW) * Constants.cityRegionTileH;
					
					var obj: SimpleGameObject = newRegion.addObject(objLvl, objType, objPlayerId, objCityId, objId, objHpPercent, objX, objY, false);					
					
					//obj may be null so we have to check it here (but we should still read all the stream)
				}
				
				newRegion.sortObjects();
			}			
			
			map.gameContainer.miniMap.objContainer.moveWithCamera(map.gameContainer.camera.miniMapX, map.gameContainer.camera.miniMapY);
		}		
	}	
}
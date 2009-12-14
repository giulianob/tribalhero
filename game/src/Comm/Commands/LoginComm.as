﻿package src.Comm.Commands {
	import flash.events.Event;
	import src.Comm.*;
	import src.Constants;
	import src.Global;
	import src.Map.*;
	import src.Objects.*;
	import src.Objects.Prototypes.*;
	import src.Objects.Factories.*;
	import src.Objects.Actions.*;
	
	public class LoginComm {
		
		private var mapComm: MapComm;
		private var map: Map;
		private var session: Session;
		
		public function LoginComm(mapComm: MapComm) {
			this.mapComm = mapComm;
			this.map = mapComm.map;
			this.session = mapComm.session;
		}
		
		public function queryXML(callback: Function, custom: * ):void
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.QUERY_XML;
			session.write(packet, callback, custom);
		}
		
		public function onLogin(packet: Packet):void
		{
			map.playerId = packet.readUInt();
			map.usernames.players.add(new Username(map.playerId, packet.readString()));
			
			var now: Date = new Date();			
			var serverTime: int = packet.readUInt();
			Constants.secondsPerUnit = Number(packet.readString());
			
			trace("Server Time is " + new Date(serverTime * 1000));
			var timeDelta: int = serverTime - int(now.time / 1000);
			trace("Delta is " + timeDelta);
			map.setTimeDelta(timeDelta);
			
			var cityCnt: int = packet.readUByte();
			for (var i: int = 0; i < cityCnt; i++)
			{			
				var id: int = packet.readUInt();				
				var name: String = packet.readString();				
				var resources: LazyResources = new LazyResources(
					new LazyValue(packet.readInt(), packet.readInt(), packet.readInt(), packet.readUInt()),
					new LazyValue(packet.readInt(), packet.readInt(), packet.readInt(), packet.readUInt()),
					new LazyValue(packet.readInt(), packet.readInt(), packet.readInt(), packet.readUInt()),
					new LazyValue(packet.readInt(), packet.readInt(), packet.readInt(), packet.readUInt()),
					new LazyValue(packet.readInt(), packet.readInt(), packet.readInt(), packet.readUInt())
				);				
				
				var radius: int = packet.readUByte();
				
				var city: City = new City(id, name, radius, resources);
				
				//Current Actions				
				var currentActionCount: int = packet.readUByte();						
				for (var k: int = 0; k < currentActionCount; k++) {
					
					var workerId: int = packet.readUInt();
					
					if (packet.readUByte() == 0)
						city.currentActions.add(new CurrentPassiveAction(workerId, packet.readUShort(), packet.readUShort(), packet.readUInt(), packet.readUInt()), false);
					else
						city.currentActions.add(new CurrentActiveAction(workerId, packet.readUShort(), packet.readUByte(), packet.readUShort(), packet.readUInt(), packet.readUInt()), false);
				}				
				city.currentActions.sort();
				
				//Notifications
				var notificationsCnt: int = packet.readUShort();
				for (k = 0; k < notificationsCnt; k++)
				{
					var notification: Notification = new Notification(packet.readUInt(), packet.readUInt(), packet.readUShort(), packet.readUShort(), packet.readUInt(), packet.readUInt());
					city.notifications.add(notification, false);
				}
				city.notifications.sort();
				
				
				//Structures					
				var structCnt: int = packet.readUShort();
				
				for (var j: int = 0; j < structCnt; j++)
				{
					var regionId: int = packet.readUShort();
					
					var objLvl: int = packet.readUByte();
					var objType: int = packet.readUShort();
					var objPlayerId: int = packet.readUInt();
					var objCityId: int = packet.readUInt();
					var objId: int = packet.readUInt();
					var objHpPercent: int = 100;
					var objX: int = packet.readUShort() + MapUtil.regionXOffset(regionId);
					var objY: int = packet.readUShort() + MapUtil.regionYOffset(regionId);
					
					var cityObj: CityObject = new CityObject(city, objId, objType, objLvl, objX, objY);													
					
					var technologyCount: int = packet.readUShort();					
					for (k = 0; k < technologyCount; k++)
						cityObj.techManager.add(new TechnologyStats(TechnologyFactory.getPrototype(packet.readUInt(), packet.readUByte()), EffectPrototype.LOCATION_OBJECT, objId));					
					
					city.objects.add(cityObj, false);
				}
				
				var troopCnt: int = packet.readUByte();
				
				for (var troopI: int = 0; troopI < troopCnt; troopI++)
				{
					var troop: Troop = new Troop();
					troop.playerId = packet.readUInt();
					troop.cityId = packet.readUInt();
					troop.id = packet.readUByte();
					troop.state = packet.readUByte();
					
					if (troop.id == 1)
						troop.upkeep = packet.readInt();
					
					switch (troop.state)
					{
						case Troop.MOVING:
						case Troop.BATTLE:
						case Troop.BATTLE_STATIONED:
						case Troop.STATIONED:
							troop.objectId = packet.readUInt();
							troop.x = packet.readUInt();
							troop.y = packet.readUInt();
							break;
					}
					
					var formationCnt: int = packet.readUByte();
					var unitType: int;
					
					for (var formationsI: int = 0; formationsI < formationCnt; formationsI++)
					{
						var formation: Formation = new Formation(packet.readUByte());
										
						troop.add(formation);							
						
						var unitCnt: int = packet.readUByte();						
					
						for (var unitI: int = 0; unitI < unitCnt; unitI++)								
							formation.add(new Unit(packet.readUShort(), packet.readUShort()));				
					}					
					
					city.troops.add(troop);
				}
				
				city.objects.sort();
				
				var templateCount: int = packet.readUShort();
				for (j = 0; j < templateCount; j++)
					city.template.add(new UnitTemplate(packet.readUShort(), packet.readUByte()));
				
				city.template.sort();									
				
				map.cities.add(city);
			}						
		}
		
	}
	
}
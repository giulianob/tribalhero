package src.Comm.Commands {
	
	import src.Comm.*;
	import src.Map.*;
	import src.Objects.*;
	import src.Objects.Actions.*;
	
	public class TroopComm {
		
		private var mapComm: MapComm;
		private var map: Map;
		private var session: Session;
		
		public function TroopComm(mapComm: MapComm) {
			this.mapComm = mapComm;
			this.map = mapComm.map;
			this.session = mapComm.session;
			
			session.addEventListener(Commands.CHANNEL_NOTIFICATION, onChannelReceive);
		}
		
		private function readTroop(packet: Packet): Troop
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
			
			return troop;
		}
		
		public function onChannelReceive(e: PacketEvent):void
		{
			switch(e.packet.cmd)
			{
				case Commands.TROOP_UPDATED:
					onCityUpdateTroop(e.packet);
					break;
				case Commands.TROOP_ADDED:
					onCityAddTroop(e.packet);
					break;
				case Commands.TROOP_REMOVED:
					onCityRemoveTroop(e.packet);
					break;
				case Commands.UNIT_TEMPLATE_UPGRADED:
					onCityTemplateUpgrade(e.packet);
			}						
		}		
		
		public function onCityTemplateUpgrade(packet: Packet):void
		{
			var cityId: int = packet.readUInt();						
			
			var city: City = map.cities.get(cityId);
			if (city == null)
				return;			
							
			city.template.clear();

			var templateCount: int = packet.readUShort();
			for (var j: int = 0; j < templateCount; j++)
				city.template.add(new UnitTemplate(packet.readUShort(), packet.readUByte()));			
			
			city.template.sort();
			
			map.doSelectedObject(map.selectedObject);
		}
		
		public function onCityUpdateTroop(packet: Packet):void
		{
			var cityId: int = packet.readUInt();						
			
			var city: City = map.cities.get(cityId);
			if (city == null)
				return;						
			
			var troop: Troop = readTroop(packet);
			
			city.troops.update(troop);
		}
		
		public function onCityAddTroop(packet: Packet):void
		{
			var cityId: int = packet.readUInt();			
			
			var city: City = map.cities.get(cityId);
			if (city == null)
				return;						
			
			var troop: Troop = readTroop(packet);
			
			city.troops.add(troop);
		}
		
		public function onCityRemoveTroop(packet: Packet):void
		{
			var cityId: int = packet.readUInt();			
			var troopId: int = packet.readUByte();
			
			var city: City = map.cities.get(cityId);
			if (city == null)
				return;						
							
			city.troops.remove(troopId);
		}		
		
		public function getTroopInfo(obj: TroopObject):void
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.TROOP_INFO;
			
			packet.writeUInt(obj.cityId);
			packet.writeUInt(obj.objectId);
			
			session.write(packet, onReceiveTroopInfo, obj);
		}
		
		public function onReceiveTroopInfo(packet: Packet, custom: * ):void
		{
			var obj: TroopObject = custom as TroopObject;
			obj.troop = new Troop();
			
			if (obj.playerId == map.playerId) {
				obj.attackRadius = obj.Radius = packet.readUByte();
				obj.speed = packet.readUByte();
					
				var formationCnt: int = packet.readUByte();
				var unitType: int;
				
				for (var formationsI: int = 0; formationsI < formationCnt; formationsI++)
				{
					var formation: Formation = new Formation(packet.readUByte());
									
					obj.troop.add(formation);							
					
					var unitCnt: int = packet.readUByte();						
				
					for (var unitI: int = 0; unitI < unitCnt; unitI++)								
						formation.add(new Unit(packet.readUShort(), packet.readUShort()));				
				}
				
				obj.template.clear();			
				var templateCnt: int = packet.readUShort();
				for (var i: int = 0; i < templateCnt; i++)
				{
					unitType = packet.readUShort();
					var unitLevel: int = packet.readUByte();																		
					obj.template.add(new UnitTemplate(unitType, unitLevel));
				}
				
				obj.actionReferences.clear();
				var currentActionCount: int = packet.readUByte();
						
				for (i = 0; i < currentActionCount; i++)				
					obj.actionReferences.add(new CurrentActionReference(packet.readUInt(), packet.readUShort()));
			}
			
			map.doSelectedObject(obj);
		}
		
		public function trainUnit(city: int, parent: int, type: int, count: int):void		
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.UNIT_TRAIN;
			packet.writeUInt(city);
			packet.writeUInt(parent);
			packet.writeUShort(type);
			packet.writeUShort(count);
		
			session.write(packet, mapComm.catchAllErrors);
		}
		
		public function upgradeUnit(cityId: int, objectId: int, type: int):void
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.UNIT_UPGRADE;
			packet.writeUInt(cityId);
			packet.writeUInt(objectId);
			packet.writeUShort(type);
			
			session.write(packet, mapComm.catchAllErrors);
		}
		
		public function retreat(city: int, troop: int):void		
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.TROOP_RETREAT;
			packet.writeUInt(city);
			packet.writeUByte(troop);
		
			session.write(packet, mapComm.catchAllErrors);
		}
		
		public function troopAttack(cityId: int, targetCityId: int, targetObjectId: int, mode: int, troop: Troop):void
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.TROOP_ATTACK;
			packet.writeUByte(mode);
			packet.writeUInt(cityId);
			packet.writeUInt(targetCityId);
			packet.writeUInt(targetObjectId);
			
			packet.writeUByte(troop.size());
			for each(var formation: Formation in troop.each())
			{				
				packet.writeUByte(formation.type);
				packet.writeUByte(formation.size());
				
				for each (var unit: Unit in formation.each())
				{														
					packet.writeUShort(unit.type);
					packet.writeUShort(unit.count);
				}
			}
			
			session.write(packet, mapComm.catchAllErrors);
		}		
		
		public function troopReinforce(cityId: int, targetCityId: int, troop: Troop):void
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.TROOP_REINFORCE;
			packet.writeUInt(cityId);
			packet.writeUInt(targetCityId);
			
			packet.writeUByte(troop.size());
			for each(var formation: Formation in troop.each())
			{				
				packet.writeUByte(formation.type);
				packet.writeUByte(formation.size());
				
				for each (var unit: Unit in formation.each())
				{														
					packet.writeUShort(unit.type);
					packet.writeUShort(unit.count);
				}
			}
			
			session.write(packet, mapComm.catchAllErrors);
		}		
		
		public function moveUnit(cityId: int, troop: Troop):void
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.LOCAL_TROOP_MOVE;
			packet.writeUInt(cityId);
			
			packet.writeUByte(troop.size());
			for each(var formation: Formation in troop.each())
			{				
				packet.writeUByte(formation.type);
				packet.writeUByte(formation.size());
				
				for each (var unit: Unit in formation.each())
				{														
					packet.writeUShort(unit.type);
					packet.writeUShort(unit.count);
				}
			}
			
			session.write(packet, mapComm.catchAllErrors);
		}	
	}
	
}
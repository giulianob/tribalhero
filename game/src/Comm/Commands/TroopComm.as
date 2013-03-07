package src.Comm.Commands {

	import src.Comm.*;
	import src.Constants;
	import src.Map.*;
	import src.Objects.*;
	import src.Objects.Actions.*;
	import src.Objects.Troop.*;
	import src.Global;
	import src.UI.Components.TroopStubGridList.TroopStubGridCell;

	public class TroopComm {

		private var mapComm: MapComm;		
		private var session: Session;

		public function TroopComm(mapComm: MapComm) {
			this.mapComm = mapComm;			
			this.session = mapComm.session;

			session.addEventListener(Commands.CHANNEL_NOTIFICATION, onChannelReceive);
		}
		
		public function dispose() : void {
			session.removeEventListener(Commands.CHANNEL_NOTIFICATION, onChannelReceive);
		}

		public function readTroop(packet: Packet): TroopStub
		{
			var troop: TroopStub = new TroopStub();
			troop.playerId = packet.readUInt();
			troop.cityId = packet.readUInt();

			troop.id = packet.readUByte();
			troop.state = packet.readUByte();			
			troop.stationedLocation = mapComm.General.readLocation(packet);
			troop.attackMode = packet.readUByte();
			troop.resources = new Resources(packet.readUInt(), packet.readUInt(), packet.readUInt(), packet.readUInt(), 0);

			
			var templateCnt: int = packet.readUByte();
			for (var templateI: int = 0; templateI < templateCnt; templateI++) {
				troop.template.add(new TroopTemplate(packet.readUShort(), packet.readUByte(), packet.readUShort(), packet.readUShort(), packet.readUByte(), packet.readUShort(), packet.readUByte(), packet.readUByte(), packet.readUByte()), false);
			}
			troop.template.sort();

			switch (troop.state)
			{
				case TroopStub.MOVING:
				case TroopStub.RETURNING_HOME:
				case TroopStub.BATTLE:
				case TroopStub.BATTLE_STATIONED:
				case TroopStub.STATIONED:
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

		public function writeTroop(stub: TroopStub, packet: Packet): void
		{
			for each(var formation: Formation in stub)
			{
				packet.writeUByte(formation.type);
				packet.writeUByte(formation.size());

				for each (var unit: Unit in formation)
				{
					packet.writeUShort(unit.type);
					packet.writeUShort(unit.count);
				}
			}
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

			var city: City = Global.map.cities.get(cityId);
			if (city == null)
			return;

			city.template.clear();

			var templateCount: int = packet.readUShort();
			for (var j: int = 0; j < templateCount; j++)
			city.template.add(new UnitTemplate(packet.readUShort(), packet.readUByte()));

			city.template.sort();

			Global.map.selectObject(Global.map.selectedObject, false);
		}

		public function onCityUpdateTroop(packet: Packet):void
		{
			var cityId: int = packet.readUInt();

			var city: City = Global.map.cities.get(cityId);
			if (city == null)
			return;

			var troop: TroopStub = readTroop(packet);

			city.troops.update(troop, [troop.cityId, troop.id]);
		}

		public function onCityAddTroop(packet: Packet):void
		{
			var cityId: int = packet.readUInt();

			var city: City = Global.map.cities.get(cityId);
			if (city == null)
			return;

			var troop: TroopStub = readTroop(packet);

			city.troops.add(troop);
		}

		public function onCityRemoveTroop(packet: Packet):void
		{
			var cityId: int = packet.readUInt();

			var troopCityId: int = packet.readUInt();
			var troopId: int = packet.readUByte();

			var city: City = Global.map.cities.get(cityId);
			if (city == null)
			return;

			city.troops.remove([troopCityId, troopId]);
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
			if (MapComm.tryShowError(packet, null, false, [400])) return;

			var obj: TroopObject = custom as TroopObject;

			obj.troop = new TroopStub();
			obj.stubId = packet.readUByte();

			if (obj.playerId == Constants.playerId) {
				obj.attackRadius = packet.readUByte();
				obj.speed = packet.readFloat();				
				obj.targetX = packet.readUInt();
				obj.targetY = packet.readUInt();

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
			}

			Global.map.selectObject(obj, false);
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

		public function troopAttackCity(cityId: int, targetCityId: int, targetObjectId: int, mode: int, troop: TroopStub, onAttackFail:Function):void
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.TROOP_ATTACK_CITY;
			packet.writeUByte(mode);
			packet.writeUInt(cityId);
			packet.writeUInt(targetCityId);
			packet.writeUInt(targetObjectId);
			writeTroop(troop, packet);

			session.write(packet, onReceiveTroopAttack, onAttackFail);
		}
		
		public function troopAttackStronghold(cityId: int, targetObjectId: int, mode: int, troop: TroopStub, onAttackFail:Function):void
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.TROOP_ATTACK_STRONGHOLD;
			packet.writeUByte(mode);
			packet.writeUInt(cityId);
			packet.writeUInt(targetObjectId);
			writeTroop(troop, packet);

			session.write(packet, onReceiveTroopAttack, onAttackFail);
		}		
		public function troopAttackBarbarian(cityId: int, targetObjectId: int, mode: int, troop: TroopStub, onAttackFail:Function):void
		{
			var packet: Packet = new Packet();
            packet.cmd = Commands.TROOP_ATTACK_BARBARIAN_TRIBE;
			packet.writeUByte(mode);
			packet.writeUInt(cityId);
			packet.writeUInt(targetObjectId);
			writeTroop(troop, packet);

			session.write(packet, onReceiveTroopAttack, onAttackFail);
		}
		
		public function onReceiveTroopAttack(packet: Packet, custom: * ):void
		{
			if ((packet.option & Packet.OPTIONS_FAILED) == Packet.OPTIONS_FAILED)
			{
				var err: int = packet.readUInt();
				GameError.showMessage(err, custom);
			}
		}
		
		public function troopReinforceCity(cityId: int, targetCityId: int, troop: TroopStub, mode: int):void
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.TROOP_REINFORCE_CITY;
			packet.writeUInt(cityId);
			packet.writeUInt(targetCityId);
			writeTroop(troop, packet);
			packet.writeUShort(mode);
			
			session.write(packet, mapComm.catchAllErrors);
		}
		
		public function troopReinforceStronghold(cityId: int, targetStrongholdId: int, troop: TroopStub, mode: int):void
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.TROOP_REINFORCE_STRONGHOLD;
			packet.writeUInt(cityId);
			packet.writeUInt(targetStrongholdId);
			writeTroop(troop, packet);
			packet.writeUShort(mode);
			
			session.write(packet, mapComm.catchAllErrors);
		}		

		public function moveUnitAndSetHideNewUnits(cityId: int, troop: TroopStub, hideNewUnits: Boolean):void
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.LOCAL_TROOP_MOVE;
			packet.writeUInt(cityId);
			
			packet.writeByte(hideNewUnits ? 1 : 0);

			writeTroop(troop, packet);

			session.write(packet, mapComm.catchAllErrors);
		}
		
		public function cityAssignmentCreate(cityId: int, targetCityId: int, targetObjectId: int, time: int, mode: int, troop: TroopStub, description: String, isAttack: Boolean): void
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.TRIBE_CITY_ASSIGNMENT_CREATE;
			
			packet.writeUByte(mode);
			packet.writeUInt(cityId);
			packet.writeUInt(targetCityId);
			packet.writeUInt(targetObjectId);
			packet.writeInt(time);			
			packet.writeUByte(isAttack?1:0);
			writeTroop(troop, packet);
			packet.writeString(description);
			
			session.write(packet, mapComm.catchAllErrors, { message: { title: "Info", content: "The assignment has been created. Other tribe members will be able to join this assignment until the end time has been reached." } });
		}
		
		public function strongholdAssignmentCreate(cityId: int, strongholdId: int, time: int, mode: int, troop: TroopStub, description: String, isAttack: Boolean): void
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.TRIBE_STRONGHOLD_ASSIGNMENT_CREATE;
			
			packet.writeUByte(mode);
			packet.writeUInt(cityId);
			packet.writeUInt(strongholdId);
			packet.writeInt(time);			
			packet.writeUByte(isAttack?1:0);
			writeTroop(troop, packet);
			packet.writeString(description);
			
			session.write(packet, mapComm.catchAllErrors, { message: { title: "Info", content: "The assignment has been created. Other tribe members will be able to join this assignment until the end time has been reached." } });
		}
		
		
		public function assignmentJoin(cityId: int, assignmentId: int, troop: TroopStub): void
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.TRIBE_ASSIGNMENT_JOIN;
			
			packet.writeUInt(cityId);
			packet.writeUInt(assignmentId);
			writeTroop(troop, packet);
			
			session.write(packet, mapComm.catchAllErrors, { message: { title: "Info", content: "You have joined the assignment. Your units will be automatically deployed at the proper time." } });
		}
		
		public function onSwitchAttackMode(packet: Packet, custom: * ): void
		{
			var stub: TroopStub = custom.stub;
			if ((packet.option & Packet.OPTIONS_FAILED) != Packet.OPTIONS_FAILED) {
				stub.attackMode = custom.mode;
            }

			mapComm.catchAllErrors(packet);
		}
		
		public function switchAttackMode(troopStub: TroopStub, mode: int): void
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.TROOP_SWITCH_MODE;
			
			packet.writeUInt(troopStub.cityId);
			packet.writeByte(troopStub.id);
			packet.writeByte(mode);
			
			session.write(packet, onSwitchAttackMode, { stub:troopStub, mode:mode } );
		}
	}

}


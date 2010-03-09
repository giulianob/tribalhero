package src.Comm.Commands {

	import src.Comm.*;
	import src.Map.*;
	import src.Objects.*;
	import src.Objects.Prototypes.*;
	import src.Objects.Factories.*;
	import src.Constants;
	import src.Objects.Actions.*;
	import flash.events.Event;
	import src.Global;
	import src.Objects.Troop.*;

	public class ObjectComm {

		private var mapComm: MapComm;
		private var session: Session;

		public function ObjectComm(mapComm: MapComm) {
			this.mapComm = mapComm;
			this.session = mapComm.session;

			session.addEventListener(Commands.CHANNEL_NOTIFICATION, onChannelReceive);
		}

		public function onChannelReceive(e: PacketEvent):void
		{
			switch(e.packet.cmd)
			{
				case Commands.OBJECT_UPDATE:
					onUpdateObject(e.packet);
				break;
				case Commands.OBJECT_ADD:
					onAddObject(e.packet);
				break;
				case Commands.OBJECT_MOVE:
					onMoveObject(e.packet);
				break;
				case Commands.OBJECT_REMOVE:
					onRemoveObject(e.packet);
				break;
				case Commands.ACTION_START:
					onReceiveStartObjectAction(e.packet);
				break;
				case Commands.ACTION_COMPLETE:
					onReceiveCompleteObjectAction(e.packet);
				break;
				case Commands.ACTION_RESCHEDULE:
					onReceiveRescheduledObjectAction(e.packet);
				break;
			}
		}

		public function defaultAction(city: int, objectid: int, command: int, values:Array):void {
			var packet: Packet = new Packet();
			packet.cmd = command;
			for each(var value:* in values) {
				switch(value.type) {
					case "Ushort":
						packet.writeUShort(value.value);
					break;
					case "Byte":
						packet.writeByte(value.value);
					break;
					case "Uint":
						packet.writeUInt(value.value);
					break;
					case "CityID":
						packet.writeUInt(city);
					break;
					case "StructureID":
						packet.writeUInt(objectid);
					break;
				}
			}

			session.write(packet, mapComm.catchAllErrors);
		}

		public function laborMove(city: int, objectid: int, value: int) :void
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.STRUCTURE_LABOR_MOVE;
			packet.writeUInt(city);
			packet.writeUInt(objectid);
			packet.writeUByte(value);

			session.write(packet, mapComm.catchAllErrors);
		}

		public function changeStructure(city: int, objectid: int, type: int, level: int):void
		{
			var packet:Packet = new Packet();
			packet.cmd = Commands.STRUCTURE_CHANGE;
			packet.writeUInt(city);
			packet.writeUInt(objectid);
			packet.writeUShort(type);
			packet.writeUByte(level);

			session.write(packet, mapComm.catchAllErrors);
		}

		public function upgradeStructure(city: int, objectid: int):void
		{
			var packet:Packet = new Packet();
			packet.cmd = Commands.STRUCTURE_UPGRADE;
			packet.writeUInt(city);
			packet.writeUInt(objectid);

			session.write(packet, mapComm.catchAllErrors);
		}

		public function buildStructure(city: int, parent: int, type: int, x: int, y: int):void
		{
			var packet:Packet = new Packet();
			packet.cmd = Commands.STRUCTURE_BUILD;
			packet.writeUInt(city);
			packet.writeUInt(parent);
			packet.writeUInt(x);
			packet.writeUInt(y);
			packet.writeUShort(type);

			session.write(packet, mapComm.catchAllErrors);
		}

		public function getPlayerUsername(id: int, callback: Function, custom: * = null) : void
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.PLAYER_USERNAME_GET;
			packet.writeUByte(1); //just doing 1 username now
			packet.writeUInt(id);

			var pass: Array = new Array();
			pass.push(callback);
			pass.push(custom);

			session.write(packet, onReceivePlayerUsername, pass);
		}

		public function onReceivePlayerUsername(packet: Packet, custom: *):void
		{
			packet.readUByte(); //just doing 1 username now

			var id: int = packet.readUInt();
			var username: String = packet.readString();

			custom[0](id, username, custom[1]);
		}

		public function getCityUsername(id: int, callback: Function, custom: * = null):void
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.CITY_USERNAME_GET;
			packet.writeUByte(1); //just doing 1 username now
			packet.writeUInt(id);

			var pass: Array = new Array();
			pass.push(callback);
			pass.push(custom);

			session.write(packet, onReceiveCityUsername, pass);
		}

		public function onReceiveCityUsername(packet: Packet, custom: *):void
		{
			packet.readUByte(); //just doing 1 username now

			var id: int = packet.readUInt();
			var username: String = packet.readString();

			custom[0](id, username, custom[1]);
		}

		public function getStructureInfo(obj: StructureObject):void
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.STRUCTURE_INFO;

			packet.writeUInt(obj.cityId);
			packet.writeUInt(obj.objectId);

			session.write(packet, onReceiveStructureInfo, obj);
		}

		public function onReceiveStructureInfo(packet: Packet, custom: *):void
		{
			if (mapComm.tryShowError(packet)) return;

			var obj:StructureObject = custom as StructureObject;

			obj.level = packet.readUByte();

			if (obj.playerId == Constants.playerId) {
				obj.labor = packet.readUByte();
				obj.hp = packet.readUShort();

				obj.clearProperties();

				var propPrototype: Array = PropertyFactory.getProperties(obj.type);

				for each (var prop: PropertyPrototype in propPrototype)
				{
					switch (prop.datatype)
					{
						case "BYTE":
							obj.addProperty(packet.readUByte().toString());
						break;
						case "USHORT":
							obj.addProperty(packet.readUShort().toString());
						break;
						case "UINT":
							obj.addProperty(packet.readUInt().toString());
						break;
						case "STRING":
							obj.addProperty(packet.readString());
						break;
						default:
							trace("Unknown datatype " + prop.datatype + " in object type " + obj.type);
						break;
					}
				}

				obj.actionReferences.clear();

				var currentActionCount: int = packet.readUByte();

				for (var i: int = 0; i < currentActionCount; i++)
				obj.actionReferences.add(new CurrentActionReference(packet.readUInt(), packet.readUShort()));
			}

			Global.map.selectObject(obj, false);
		}

		public function onUpdateObject(packet: Packet):void
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
			var objState: int = packet.readUByte();

			var obj: SimpleGameObject = Global.map.regions.updateObject(regionId, objPlayerId, objCityId, objId, objType, objLvl, objHpPercent, objX, objY);

			if (objState > 0)
			{
				switch(objState)
				{
					case SimpleGameObject.STATE_BATTLE:
						var battleCityId: int = packet.readUInt();
						if (obj) obj.battleCityId = battleCityId;
						else trace("Receive battle id for unknown object");
					break;
					default:
						trace("Unknown object state in onReceiveRegion:" + objState);
					break;
				}
			}

			if (objId == 1) { // main building
				var radius: int = packet.readUByte();
				if (obj) obj.wall.draw(radius);
			}

			if (obj)
			{
				obj.State = objState;
				obj.dispatchEvent(new Event(SimpleGameObject.OBJECT_UPDATE));

				if (obj as GameObject != null && obj == Global.map.selectedObject)
				{
					Global.map.selectObject(obj as GameObject);
				}
			}
		}

		public function onAddObject(packet: Packet):void
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
			var obj: SimpleGameObject = Global.map.regions.addObject(null, regionId, objLvl, objType, objPlayerId, objCityId, objId, objHpPercent, objX, objY);
			var objState: int = packet.readUByte();

			if (objId == 1) { // main building
				var radius: int = packet.readUByte();
				if (obj) obj.wall.draw(radius);
			}

			if (obj) {
				obj.State = objState;
				obj.fadeIn();
			}
		}

		public function onRemoveObject(packet: Packet):void
		{
			var regionId: int = packet.readUShort();
			var cityId: int = packet.readUInt();
			var objId: int = packet.readUInt();

			Global.map.regions.removeObject(regionId, cityId, objId);
		}

		public function onMoveObject(packet: Packet):void
		{
			var oldRegionId: int = packet.readUShort();
			var newRegionId: int = packet.readUShort();

			var objLvl: int = packet.readUByte();
			var objType: int = packet.readUShort();
			var objPlayerId: int = packet.readUInt();
			var objCityId: int = packet.readUInt();
			var objId: int = packet.readUInt();
			var objHpPercent: int = 100;
			var objX: int = packet.readUShort() + (newRegionId % Constants.mapRegionW) * Constants.regionTileW;
			var objY: int = packet.readUShort() + int(newRegionId / Constants.mapRegionW) * Constants.regionTileH;
			var objState: int = packet.readUByte();

			var obj: SimpleGameObject = Global.map.regions.moveObject(oldRegionId, newRegionId, objLvl, objType, objPlayerId, objCityId, objId, objHpPercent, objX, objY);

			if (!obj)
			return;

			if (objState > 0)
			{
				switch(objState)
				{
					case SimpleGameObject.STATE_BATTLE:
						obj.battleCityId = packet.readUInt();;
					break;
					default:
						trace("Unknown object state in onReceiveRegion:" + objState);
					break;
				}
			}

			if (objId == 1) { // main building
				var radius: int = packet.readUByte();
				obj.wall.draw(radius);
			}

			obj.State = objState;
			obj.dispatchEvent(new Event(SimpleGameObject.OBJECT_UPDATE));
		}

		public function onReceiveStartObjectAction(packet: Packet):void
		{
			var cityId: int = packet.readUInt();
			var objId: int = packet.readUInt();
			var currentAction: CurrentAction;
			trace("Got new actionz");
			if (packet.readUByte() == 0)
			currentAction = new CurrentPassiveAction(objId, packet.readUShort(), packet.readUShort(), packet.readUInt(), packet.readUInt());
			else
			currentAction = new CurrentActiveAction(objId, packet.readUShort(), packet.readUByte(), packet.readUShort(), packet.readUInt(), packet.readUInt());

			var city: City = Global.map.cities.get(cityId);
			if (city == null)
			return;

			city.currentActions.add(currentAction);
		}

		public function onReceiveRescheduledObjectAction(packet: Packet):void
		{
			var cityId: int = packet.readUInt();
			var objId: int = packet.readUInt();
			var currentAction: *;
			var mode: int = packet.readUByte();
			var actionId: int = packet.readUShort();

			var city: City = Global.map.cities.get(cityId);
			if (city == null)
			return;

			currentAction = city.currentActions.get(actionId);

			if (!currentAction)
			return;

			if (mode == 0)
			{
				currentAction.type = packet.readUShort();
			}
			else
			{
				currentAction.index = packet.readUByte();
				currentAction.count = packet.readUShort();
			}

			currentAction.startTime = packet.readUInt();
			currentAction.endTime = packet.readUInt();
		}

		public function onReceiveCompleteObjectAction(packet: Packet):void
		{
			var cityId: int = packet.readUInt();
			var objId: int = packet.readUInt();

			var currentAction: CurrentAction;

			if (packet.readUByte() == 0)
			currentAction = new CurrentPassiveAction(objId, packet.readUShort(), packet.readUShort(), packet.readUInt(), packet.readUInt());
			else
			currentAction = new CurrentActiveAction(objId, packet.readUShort(), packet.readUByte(), packet.readUShort(), packet.readUInt(), packet.readUInt());

			var city: City = Global.map.cities.get(cityId);
			if (city == null)
			return;

			city.currentActions.remove(currentAction.id);
		}

		public function cancelAction(cityId: int, objectId: int, id: int):void
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.ACTION_CANCEL;
			packet.writeUInt(cityId);
			packet.writeUInt(objectId);
			packet.writeUShort(id);

			session.write(packet, onReceiveCancelAction, objectId);
		}

		public function onReceiveCancelAction(packet: Packet, custom: *):void
		{
		}

	}

}


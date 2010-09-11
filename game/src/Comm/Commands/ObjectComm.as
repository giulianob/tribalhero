package src.Comm.Commands {

	import org.aswing.AssetIcon;
	import src.Comm.*;
	import src.Map.*;
	import src.Objects.*;
	import src.Objects.Prototypes.*;
	import src.Objects.Factories.*;
	import src.Constants;
	import src.Objects.Actions.*;
	import flash.events.Event;
	import src.Global;
	import src.Objects.States.BattleState;
	import src.Objects.States.GameObjectState;
	import src.Objects.States.MovingState;
	import src.Objects.Troop.*;
	import src.UI.Components.ScreenMessages.*;

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

		public function readState(obj: SimpleGameObject, packet: Packet, objState: int) : void {
			switch(objState)
			{
				case SimpleGameObject.STATE_NORMAL:
					if (obj) obj.State = new GameObjectState();
				break;
				case SimpleGameObject.STATE_BATTLE:
					var battleCityId: int = packet.readUInt();
					if (obj) obj.State = new BattleState(battleCityId);
				break;
				case SimpleGameObject.STATE_MOVING:
					var destX: int = packet.readUInt();
					var destY: int = packet.readUInt();
					if (obj) obj.State = new MovingState(destX, destY);
				break;
				default:
					trace("Unknown object state in onReceiveRegion:" + objState);
				break;
			}
		}

		public function readWall(obj: SimpleGameObject, packet: Packet) : void {
			if (ObjectFactory.getClassType(obj.type) == ObjectFactory.TYPE_STRUCTURE && obj.objectId == 1) {
				var radius: int = packet.readUByte();
				if (obj) obj.wall.draw(radius);
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

		public function downgrade(city: int, objectId: int, targetId: int) :void
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.STRUCTURE_DOWNGRADE;
			packet.writeUInt(city);
			packet.writeUInt(objectId);
			packet.writeUInt(targetId);

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

		public function getForestInfo(obj: Forest):void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.FOREST_INFO;

			packet.writeUInt(obj.objectId);

			session.write(packet, onReceiveForestInfo, obj);
		}

		public function onReceiveForestInfo(packet: Packet, custom: * ) : void {
			if (MapComm.tryShowError(packet)) return;

			var forest: Forest = custom as Forest;

			forest.rate = packet.readFloat();
			forest.labor = packet.readInt();
			forest.depleteTime = packet.readUInt();
			forest.wood = new AggressiveLazyValue(packet.readInt(), packet.readInt(), packet.readInt(), packet.readInt(), packet.readUInt());

			Global.map.selectObject(forest, false);
		}

		public function createForestCamp(forestId: int, cityId: int, type: int, labor: int) : void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.FOREST_CAMP_CREATE;

			packet.writeUInt(cityId);
			packet.writeUInt(forestId);
			packet.writeUShort(type);
			packet.writeUByte(labor);

			session.write(packet, mapComm.catchAllErrors);
		}

		public function removeForestCamp(cityId: int, campId: int) : void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.FOREST_CAMP_REMOVE;

			packet.writeUInt(cityId);
			packet.writeUInt(campId);

			session.write(packet, mapComm.catchAllErrors);
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
			if (MapComm.tryShowError(packet)) return;

			var obj:StructureObject = custom as StructureObject;

			obj.clearProperties();

			obj.level = packet.readUByte();

			if (obj.playerId == Constants.playerId) {
				obj.labor = packet.readUByte();
				obj.hp = packet.readUShort();

				var propPrototype: Array = PropertyFactory.getAllProperties(obj.type);
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
						case "INT":
							obj.addProperty(packet.readInt());
						break;
						default:
							trace("Unknown datatype " + prop.datatype + " in object type " + obj.type);
						break;
					}
				}

				obj.actionReferences.clear();

				var currentActionCount: int = packet.readUByte();

				for (var i: int = 0; i < currentActionCount; i++)
				obj.actionReferences.add(new CurrentActionReference(packet.readUInt(), packet.readUInt()));
			}
			else {
				propPrototype = PropertyFactory.getProperties(obj.type, PropertyPrototype.VISIBILITY_PUBLIC);

				for each (prop in propPrototype)
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
						case "INT":
							obj.addProperty(packet.readInt());
						break;
						case "FLOAT":
							obj.addProperty(packet.readFloat());
						default:
							trace("Unknown datatype " + prop.datatype + " in object type " + obj.type);
						break;
					}
				}
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

			if (!obj) return;

			readState(obj, packet, objState);

			readWall(obj, packet);

			obj.dispatchEvent(new Event(SimpleGameObject.OBJECT_UPDATE));
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

			if (!obj) return;

			readState(obj, packet, objState);

			readWall(obj, packet);

			obj.fadeIn();
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

			if (!obj) return;

			readState(obj, packet, objState);

			readWall(obj, packet);

			obj.dispatchEvent(new Event(SimpleGameObject.OBJECT_UPDATE));
		}

		public function onReceiveStartObjectAction(packet: Packet):void
		{
			var cityId: int = packet.readUInt();
			var objId: int = packet.readUInt();
			var currentAction: CurrentAction;

			if (packet.readUByte() == 0)
			currentAction = new CurrentPassiveAction(objId, packet.readUInt(), packet.readUShort(), packet.readUInt(), packet.readUInt());
			else
			currentAction = new CurrentActiveAction(objId, packet.readUInt(), packet.readUByte(), packet.readUShort(), packet.readUInt(), packet.readUInt());
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
			var actionId: int = packet.readUInt();

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
			var state: int = packet.readInt();
			var cityId: int = packet.readUInt();
			var objId: int = packet.readUInt();

			var currentAction: CurrentAction;

			if (packet.readUByte() == 0) currentAction = new CurrentPassiveAction(objId, packet.readUInt(), packet.readUShort(), packet.readUInt(), packet.readUInt());
			else currentAction = new CurrentActiveAction(objId, packet.readUInt(), packet.readUByte(), packet.readUShort(), packet.readUInt(), packet.readUInt());

			var city: City = Global.map.cities.get(cityId);
			if (city == null) return;

			// Show screen message if action completes
			if (state == Action.STATE_COMPLETED) {
				var obj: CityObject = city.objects.get(objId);
				if (obj) {
					var strPrototype: StructurePrototype = StructureFactory.getPrototype(obj.getType(), obj.getLevel());
					Global.gameContainer.screenMessage.addMessage(new ScreenMessageItem("/ACTCMPT/" + city.id + "/" + objId + "/" + currentAction.id, city.name + " " + strPrototype.getName() + ": " + currentAction.toString(obj) + " has completed", new AssetIcon(new ICON_CLOCK), 2000));
				}
			}

			city.currentActions.remove(currentAction.id);
		}

		public function cancelAction(cityId: int, objectId: int, id: int):void
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.ACTION_CANCEL;
			packet.writeUInt(cityId);
			packet.writeUInt(objectId);
			packet.writeUInt(id);

			session.write(packet, mapComm.catchAllErrors, objectId);
		}

	}

}


package src.Comm.Commands {

	import flash.geom.*;
	import org.aswing.*;
	import src.*;
	import src.Comm.*;
	import src.Map.*;
	import src.Objects.*;
	import src.Objects.Actions.*;
	import src.Objects.Factories.*;
	import src.Objects.Prototypes.*;
	import src.Objects.States.*;
	import src.Objects.Troop.*;
	import src.UI.Components.ScreenMessages.*;
	import src.Util.*;

	public class ObjectComm {

		private var mapComm: MapComm;
		private var session: Session;

		public function ObjectComm(mapComm: MapComm) {
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
		
		public function readObject(packet: Packet, regionId: int, forRegion: Boolean = false) : * {
			var obj: * = {
				type: packet.readUShort(),
				x: packet.readUShort() + MapUtil.regionXOffset(regionId),
				y: packet.readUShort() + MapUtil.regionYOffset(regionId),	
				groupId: packet.readUInt(),
				id: packet.readUInt()
			};
			
			switch(ObjectFactory.getClassType(obj.type)) {
				case ObjectFactory.TYPE_STRUCTURE:
					obj.playerId = packet.readUInt();
					obj.lvl = packet.readUByte();		
					obj.labor = forRegion ? 0 : packet.readUShort();
						
					if (obj.id == 1) {
						obj.wallRadius = packet.readUByte();
					}
					break;
				case ObjectFactory.TYPE_FOREST:
					obj.lvl = packet.readUByte();						
					break;
				case ObjectFactory.TYPE_TROOP_OBJ:
					obj.playerId = packet.readUInt();
					break;
				case ObjectFactory.TYPE_STRONGHOLD:
					obj.lvl = packet.readUByte();
					obj.tribeId = packet.readUInt();
					break;
				case ObjectFactory.TYPE_BARBARIAN_TRIBE:
					obj.lvl = packet.readUByte();
					break;
			}
			
			obj.state = readState(packet);
			
			return obj;
		}
			
		public function readObjectInstance(packet: Packet, regionId: int, forRegion: Boolean = false): SimpleGameObject {
			var obj: * = readObject(packet, regionId, forRegion);
			
			var coord: Point = MapUtil.getScreenCoord(obj.x, obj.y);
			
			switch(ObjectFactory.getClassType(obj.type)) {
				case ObjectFactory.TYPE_STRUCTURE:
					return StructureFactory.getInstance(obj.type, obj.state, coord.x, coord.y, obj.playerId, obj.groupId, obj.id, obj.lvl, obj.wallRadius);
				case ObjectFactory.TYPE_FOREST:
					return ForestFactory.getInstance(obj.type, obj.state, coord.x, coord.y, obj.groupId, obj.id, obj.lvl);
				case ObjectFactory.TYPE_TROOP_OBJ:
					return TroopFactory.getInstance(obj.type, obj.state, coord.x, coord.y, obj.playerId, obj.groupId, obj.id);
				case ObjectFactory.TYPE_STRONGHOLD:
					return StrongholdFactory.getInstance(obj.type, obj.state, coord.x, coord.y, obj.groupId, obj.id, obj.lvl, obj.tribeId);
				case ObjectFactory.TYPE_BARBARIAN_TRIBE:
					return BarbarianTribeFactory.getInstance(obj.type, obj.state, coord.x, coord.y, obj.groupId, obj.id, obj.lvl);
				default:
					throw new Error("Trying to unread unknown object class type " + obj.type);
			}
		}

		public function readState(packet: Packet) : GameObjectState {
			var objState: int = packet.readUByte();
			
			switch(objState)
			{
				case SimpleGameObject.STATE_NORMAL:
					return new GameObjectState();
				case SimpleGameObject.STATE_BATTLE:
					var battleId: int = packet.readUInt();
					return new BattleState(battleId);
				case SimpleGameObject.STATE_MOVING:
					return new MovingState();
				default:
					throw new Error("Unknown object state in onReceiveRegion:" + objState);
			}
			
			return null;
		}

		public function readWall(obj: StructureObject, packet: Packet) : void {
			if (obj.objectId == 1)
				obj.wallManager.draw(packet.readUByte());			
		}

		public function defaultAction(city: int, objectid: int, command: int):void {
			var packet: Packet = new Packet();
			packet.cmd = command;
			packet.writeUInt(city);
			packet.writeUInt(objectid);
			session.write(packet, mapComm.catchAllErrors);
		}

		public function laborMove(city: int, objectid: int, value: int) :void
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.STRUCTURE_LABOR_MOVE;
			packet.writeUInt(city);
			packet.writeUInt(objectid);
			packet.writeUShort(value);

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

		public function buildStructure(city: int, parent: int, type: int, level: int, x: int, y: int):void
		{
			var packet:Packet = new Packet();
			packet.cmd = Commands.STRUCTURE_BUILD;
			packet.writeUInt(city);
			packet.writeUInt(parent);
			packet.writeUInt(x);
			packet.writeUInt(y);
			packet.writeUShort(type);
			packet.writeUByte(level);

			session.write(packet, mapComm.catchAllErrors);
		}

		public function getPlayerUsernameFromCityName(cityName: String, callback: Function) : void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.PLAYER_NAME_FROM_CITY_NAME;
			packet.writeString(cityName);
			
			session.write(packet, callback);
		}

		public function getTribeUsername(id: int, callback: Function, custom: * = null) : void
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.TRIBE_USERNAME_GET;
			packet.writeUByte(1); //just doing 1 username now
			packet.writeUInt(id);

			var pass: Array = new Array();
			pass.push(callback);
			pass.push(custom);

			session.write(packet, onReceiveTribeUsername, pass);
		}

		public function onReceiveTribeUsername(packet: Packet, custom: *):void
		{
			packet.readUByte(); //just doing 1 username now

			var id: int = packet.readUInt();
			var username: String = packet.readString();

			custom[0](id, username, custom[1]);
		}
		
		public function getStrongholdUsername(id: int, callback: Function, custom: * = null) : void
		{
			if (id <= 0) {
				callback(id, "System", custom);
				return;
			}
			
			var packet: Packet = new Packet();
			packet.cmd = Commands.STRONGHOLD_USERNAME_GET;
			packet.writeUByte(1); //just doing 1 username now
			packet.writeUInt(id);

			var pass: Array = new Array();
			pass.push(callback);
			pass.push(custom);
			pass.push(id);

			session.write(packet, onReceivePlayerUsername, pass);
		
		}

		public function onReceiveStrongholdUsername(packet: Packet, custom: *):void
		{
			packet.readUByte(); //just doing 1 username now

			var id: int = packet.readUInt();
			var username: String = packet.readString();

			custom[0](id, username, custom[1]);
		}
		
		public function getPlayerUsername(id: int, callback: Function, custom: * = null) : void
		{
			if (id <= 0) {
				callback(id, "System", custom);
				return;
			}
			
			var packet: Packet = new Packet();
			packet.cmd = Commands.PLAYER_USERNAME_GET;
			packet.writeUByte(1); //just doing 1 username now
			packet.writeUInt(id);

			var pass: Array = new Array();
			pass.push(callback);
			pass.push(custom);
			pass.push(id);

			session.write(packet, onReceivePlayerUsername, pass);
		}

		public function onReceivePlayerUsername(packet: Packet, custom: *):void
		{
			if ((packet.option & Packet.OPTIONS_FAILED) == Packet.OPTIONS_FAILED) {
				custom[0](custom[2], "", custom[1]);
				return;
			}
			
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
			if ((packet.option & Packet.OPTIONS_FAILED) == Packet.OPTIONS_FAILED) {
				return;
			}
			
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

		public function structureSelfDestroy(cityId: int, structureId: int) : void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.STRUCTURE_SELF_DESTROY;

			packet.writeUInt(cityId);
			packet.writeUInt(structureId);

			session.write(packet, mapComm.catchAllErrors);
		}		
		
		public function gatherResource(cityId: int, structureId: int) : void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.RESOURCE_GATHER;

			packet.writeUInt(cityId);
			packet.writeUInt(structureId);

			session.write(packet, mapComm.catchAllErrors);
		}		
		
		public function createForestCamp(forestId: int, cityId: int, type: int, labor: int) : void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.FOREST_CAMP_CREATE;

			packet.writeUInt(cityId);
			packet.writeUInt(forestId);
			packet.writeUShort(type);
			packet.writeUShort(labor);

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
			if (MapComm.tryShowError(packet, null, false, [400])) return;

			var obj:StructureObject = custom as StructureObject;

			obj.clearProperties();

			obj.type = packet.readUShort();
			obj.level = packet.readUByte();

			if (obj.playerId == Constants.playerId) {
				obj.labor = packet.readUShort();
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
							Util.log("Unknown datatype " + prop.datatype + " in object type " + obj.type);
						break;
					}
				}
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
							Util.log("Unknown datatype " + prop.datatype + " in object type " + obj.type);
						break;
					}
				}
			}

			Global.map.selectObject(obj, false);
		}

		public function onUpdateObject(packet: Packet):void
		{
			var regionId: int = packet.readUShort();
			
			var obj: SimpleGameObject = readObjectInstance(packet, regionId);
			
			Global.map.regions.updateObject(regionId, obj);
		}

		public function onAddObject(packet: Packet):void
		{
			var regionId: int = packet.readUShort();

			var obj: SimpleGameObject = readObjectInstance(packet, regionId);
			
			Global.map.regions.addObject(regionId, obj);			
		}

		public function onRemoveObject(packet: Packet):void
		{
			var regionId: int = packet.readUShort();
			var groupId: int = packet.readUInt();
			var objId: int = packet.readUInt();

			Global.map.regions.removeObject(regionId, groupId, objId);
		}

		public function onMoveObject(packet: Packet):void
		{
			var oldRegionId: int = packet.readUShort();
			var newRegionId: int = packet.readUShort();
			
			var obj: SimpleGameObject = readObjectInstance(packet, newRegionId);
			
			Global.map.regions.moveObject(oldRegionId, newRegionId, obj);
		}

		public function onReceiveStartObjectAction(packet: Packet):void
		{
			var cityId: int = packet.readUInt();
			var objId: int = packet.readUInt();
			var currentAction: CurrentAction;

			if (packet.readUByte() == 0) 
				currentAction = new CurrentPassiveAction(objId, packet.readUInt(), packet.readUShort(), packet.readString(), packet.readUInt(), packet.readUInt());
			else 
				currentAction = new CurrentActiveAction(objId, packet.readUInt(), packet.readInt(), packet.readUByte(), packet.readUShort(), packet.readUInt(), packet.readUInt());
			
			var city: City = Global.map.cities.get(cityId);
			if (city == null)
				return;
				
			city.currentActions.add(currentAction);
			
			if (!Global.map.selectedObject)
				Global.map.selectWhenViewable(cityId, objId);
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

			if (mode == 0) {
				currentAction.type = packet.readUShort();			
				currentAction.nlsDescription = packet.readString();
			}
			else
			{
				currentAction.workerType = packet.readInt();
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

			if (packet.readUByte() == 0) 
				currentAction = new CurrentPassiveAction(objId, packet.readUInt(), packet.readUShort(), packet.readString(), packet.readUInt(), packet.readUInt());
			else 
				currentAction = new CurrentActiveAction(objId, packet.readUInt(), packet.readInt(), packet.readUByte(), packet.readUShort(), packet.readUInt(), packet.readUInt());

			var city: City = Global.map.cities.get(cityId);
			if (city == null) 
				return;

			// Show screen message if action completes
			if (state == Action.STATE_COMPLETED) {
				var obj: CityObject = city.objects.get(objId);
				if (obj) {
					var strPrototype: StructurePrototype = StructureFactory.getPrototype(obj.type, obj.level);
					Global.gameContainer.screenMessage.addMessage(new ScreenMessageItem("/ACTCMPT/" + city.id + "/" + objId + "/" + currentAction.id, city.name + " " + strPrototype.getName() + ": " + currentAction.toString() + " has completed", new AssetIcon(new ICON_CLOCK), 60000));
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


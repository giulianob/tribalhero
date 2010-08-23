package src.Comm.Commands {
	import flash.geom.Point;
	import src.Comm.*;
	import src.Global;
	import src.Map.*;
	import src.Objects.*;
	import flash.events.Event;
	import src.Objects.Actions.Notification;
	import src.Objects.Prototypes.*;
	import src.Objects.Effects.*;
	import src.Objects.Factories.*;
	import src.UI.Components.ScreenMessages.BuiltInMessages;

	public class CityComm {

		private var mapComm: MapComm;
		private var session: Session;

		public function CityComm(mapComm: MapComm) {
			this.mapComm = mapComm;
			this.session = mapComm.session;

			session.addEventListener(Commands.CHANNEL_NOTIFICATION, onChannelReceive);
		}

		public function onChannelReceive(e: PacketEvent):void
		{
			switch(e.packet.cmd)
			{
				case Commands.CITY_RESOURCES_UPDATE:
					onCityResourcesUpdate(e.packet);
				break;
				case Commands.CITY_OBJECT_ADD:
					onCityAddObject(e.packet);
				break;
				case Commands.CITY_OBJECT_REMOVE:
					onCityRemoveObject(e.packet);
				break;
				case Commands.CITY_OBJECT_UPDATE:
					onCityUpdateObject(e.packet);
				break;
				case Commands.CITY_RADIUS_UPDATE:
					onCityRadiusUpdate(e.packet);
				break;
				case Commands.NOTIFICATION_ADD:
				case Commands.NOTIFICATION_UPDATE:
					onReceiveUpdateNotification(e.packet);
				break;
				case Commands.NOTIFICATION_REMOVE:
					onReceiveRemoveNotification(e.packet);
				break;
				case Commands.TECHNOLOGY_ADDED:
				case Commands.TECHNOLOGY_REMOVED:
				case Commands.TECHNOLOGY_UPGRADED:
					onReceiveTechnologyChanged(e.packet);
				break;
				case Commands.CITY_ATTACK_DEFENSE_POINT_UPDATE:
					onReceiveDefenseAttackPoint(e.packet);
				break;
				case Commands.CITY_BATTLE_ENDED:
				case Commands.CITY_BATTLE_STARTED:
					onReceiveBattleStateChange(e.packet);
				break;
			}
		}

		public function onReceiveDefenseAttackPoint(packet: Packet): void {
			var cityId: int = packet.readUInt();
			var attackPoint: int = packet.readInt();
			var defensePoint: int = packet.readInt();

			var city: City = Global.map.cities.get(cityId);

			if (city != null) {
				city.attackPoint = attackPoint;
				city.defensePoint = defensePoint;
			}
		}

		public function onReceiveBattleStateChange(packet: Packet): void {
			var cityId: int = packet.readUInt();

			var city: City = Global.map.cities.get(cityId);

			if (city != null) {
				city.inBattle = packet.cmd == Commands.CITY_BATTLE_STARTED;

				BuiltInMessages.showInBattle(city);
			}
		}

		public function onCityRadiusUpdate(packet: Packet):void
		{
			var cityId: int = packet.readUInt();
			var radius: int = packet.readUByte();

			var city: City = Global.map.cities.get(cityId);

			if (city != null)
			{
				city.radius = radius;
				city.dispatchEvent(new Event(City.RADIUS_UPDATE));
			}
		}

		public function onCityResourcesUpdate(packet: Packet):void
		{
			var cityId: int = packet.readUInt();
			var resources: LazyResources = new LazyResources(
			new LazyValue(packet.readInt(), packet.readInt(), packet.readInt(), packet.readInt(), packet.readUInt()),
			new LazyValue(packet.readInt(), packet.readInt(), 0, packet.readInt(), packet.readUInt()),
			new LazyValue(packet.readInt(), packet.readInt(), 0, packet.readInt(), packet.readUInt()),
			new LazyValue(packet.readInt(), packet.readInt(), 0, packet.readInt(), packet.readUInt()),
			new LazyValue(packet.readInt(), packet.readInt(), 0, packet.readInt(), packet.readUInt())
			);

			var city: City = Global.map.cities.get(cityId);

			if (city != null)
			{
				city.resources = resources;
				city.dispatchEvent(new Event(City.RESOURCES_UPDATE));

				BuiltInMessages.showTroopsStarving(city);
			}
		}

		public function onCityUpdateObject(packet: Packet):void
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
			var objLabor: int = 0;
			if (ObjectFactory.getClassType(objType) == ObjectFactory.TYPE_STRUCTURE)
			objLabor = packet.readUByte();

			var city: City = Global.map.cities.get(objCityId);

			if (city == null)
			{
				trace("Received channel city obj update command for unknown city");
				return;
			}

			var obj: CityObject = city.objects.get(objId);

			if (obj == null)
			return;

			obj.labor = objLabor;
			obj.level = objLvl;
			obj.x = objX;
			obj.y = objY;
		}

		public function onCityAddObject(packet: Packet):void
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
			var objLabor: int = 0;
			if (ObjectFactory.getClassType(objType) == ObjectFactory.TYPE_STRUCTURE)
			objLabor = packet.readUByte();

			var city: City = Global.map.cities.get(objCityId);

			if (city == null)
			{
				trace("Received channel city obj add command for unknown city");
				return;
			}

			city.objects.add(new CityObject(city, objId, objType, objLvl, objX, objY, objLabor));
		}

		public function onCityRemoveObject(packet: Packet):void
		{
			var objCityId: int = packet.readUInt();
			var objId: int = packet.readUInt();
			var city: City = Global.map.cities.get(objCityId);

			if (city == null)
			{
				trace("Received channel city obj remove command for unknown city");
				return;
			}

			city.objects.remove(objId);
		}

		public function onReceiveTechnologyChanged(packet: Packet):void
		{
			var cityId: int = packet.readUInt();
			var ownerId: int = packet.readUInt();
			var techType: int = packet.readUInt();
			var techLevel: int = packet.readUByte();

			var ownerLocation: int = EffectPrototype.LOCATION_OBJECT;

			if (ownerId == 0)
			{
				ownerId = cityId;
				ownerLocation = EffectPrototype.LOCATION_CITY;
			}

			var techStats: TechnologyStats = new TechnologyStats(TechnologyFactory.getPrototype(techType, techLevel), ownerLocation, ownerId);

			var city: City = Global.map.cities.get(cityId);

			if (city == null) {
				trace("Received technology notification for unknown city");
				return;
			}

			if (ownerLocation == EffectPrototype.LOCATION_OBJECT)
			{
				var cityObj: CityObject = city.objects.get(ownerId);

				if (cityObj == null) {
					trace("Received technology notification for unknown city object");
					return;
				}

				switch(packet.cmd)
				{
					case Commands.TECHNOLOGY_UPGRADED:
						cityObj.techManager.update(techStats);
					break;
					case Commands.TECHNOLOGY_ADDED:
						cityObj.techManager.add(techStats);
					break;
					case Commands.TECHNOLOGY_REMOVED:
						cityObj.techManager.remove(techStats);
					break;
				}
			}
			else if (ownerLocation == EffectPrototype.LOCATION_CITY)
			{
				switch(packet.cmd)
				{
					case Commands.TECHNOLOGY_UPGRADED:
						city.techManager.update(techStats);
					break;
					case Commands.TECHNOLOGY_ADDED:
						city.techManager.add(techStats);
					break;
					case Commands.TECHNOLOGY_REMOVED:
						city.techManager.remove(techStats);
					break;
				}
			}

			if (Global.map.selectedObject != null)
			Global.map.selectedObject.dispatchEvent(new Event(SimpleGameObject.OBJECT_UPDATE));
		}

		public function sendResources(resources: Resources, cityId: int, objId: int, targetCityId: int):void
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.CITY_RESOURCES_SEND;
			packet.writeUInt(cityId);
			packet.writeUInt(objId);
			packet.writeUInt(targetCityId);
			packet.writeInt(resources.crop);
			packet.writeInt(resources.gold);
			packet.writeInt(resources.iron);
			packet.writeInt(resources.wood);

			session.write(packet, mapComm.catchAllErrors);
		}

		public function technologyUpgrade(cityId: int, parent: int, type: int):void
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.TECHNOLOGY_UPGRADE;
			packet.writeUInt(cityId);
			packet.writeUInt(parent);
			packet.writeUInt(type);

			session.write(packet, mapComm.catchAllErrors);
		}

		public function onReceiveUpdateNotification(packet: Packet):void
		{
			var cityId: int = packet.readUInt();

			var notification: Notification = new Notification(packet.readUInt(), packet.readUInt(), packet.readUInt(), packet.readUShort(), packet.readUInt(), packet.readUInt());

			var city: City = Global.map.cities.get(cityId);
			if (city == null)
			return;

			if (packet.cmd == Commands.NOTIFICATION_UPDATE) {
				city.notifications.remove( [ notification.cityId, notification.actionId ] );
			}

			city.notifications.add(notification);

			BuiltInMessages.showIncomingAttack(city);
		}

		public function onReceiveRemoveNotification(packet: Packet):void
		{
			var city: City = Global.map.cities.get(packet.readUInt());

			if (city == null)
			return;

			city.notifications.remove( [ packet.readUInt(), packet.readUInt() ] );

			BuiltInMessages.showIncomingAttack(city);
		}

		public function gotoNotificationLocation(srcCityId: int, cityId: int, actionId: int) : void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.NOTIFICATION_LOCATE;
			packet.writeUInt(srcCityId);
			packet.writeUInt(cityId);
			packet.writeUShort(actionId);

			session.write(packet, onReceiveNotificationLocation);
		}

		public function onReceiveNotificationLocation(packet: Packet, custom: *):void
		{
			var pt: Point = MapUtil.getScreenCoord(packet.readUInt(), packet.readUInt());
			Global.map.camera.ScrollToCenter(pt.x, pt.y);
		}

		public function gotoCityLocation(cityId: int) : void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.CITY_LOCATE;
			packet.writeUInt(cityId);

			session.write(packet, onReceiveCityLocation);
		}
		
		public function gotoCityLocationByName(cityName: String) : void {
			var packet: Packet = new Packet();
			packet.cmd = Commands.CITY_LOCATE_BY_NAME;
			packet.writeString(cityName);

			session.write(packet, onReceiveCityLocation);
		}		

		public function onReceiveCityLocation(packet: Packet, custom: * ): void {
			if (mapComm.tryShowError(packet)) return;			
			var pt: Point = MapUtil.getScreenCoord(packet.readUInt(), packet.readUInt());
			Global.map.camera.ScrollToCenter(pt.x, pt.y);
		}
	}

}


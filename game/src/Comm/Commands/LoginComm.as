package src.Comm.Commands {
	import src.Comm.*;
	import src.Constants;
	import src.Global;
	import src.Map.*;
	import src.Objects.*;
	import src.Objects.Prototypes.*;
	import src.Objects.Factories.*;
	import src.Objects.Actions.*;
	import src.Objects.Troop.*;
	import src.UI.Dialog.InfoDialog;

	public class LoginComm {

		private var mapComm: MapComm;
		private var session: Session;

		public function LoginComm(mapComm: MapComm) {
			this.mapComm = mapComm;
			this.session = mapComm.session;
		}

		public function queryXML(callback: Function, custom: * ):void
		{
			var packet: Packet = new Packet();
			packet.cmd = Commands.QUERY_XML;
			session.write(packet, callback, custom);
		}

		public function createInitialCity(name: String, onCityCreated: Function) : void {
			var pnlLoading: InfoDialog = InfoDialog.showMessageDialog("Creating city", "We're creating your city...", null, null, true, false, 0);

			var packet: Packet = new Packet();
			packet.cmd = Commands.CITY_CREATE_INITIAL;
			packet.writeString(name);

			session.write(packet, onCreateInitialCity, [pnlLoading, onCityCreated]);
		}

		private function onCreateInitialCity(packet: Packet, custom: * ) : void {
			(custom[0] as InfoDialog).getFrame().dispose();

			if ((packet.option & Packet.OPTIONS_FAILED) == Packet.OPTIONS_FAILED) {
				GameError.showMessage(packet.readUInt());
				return;
			}

			custom[1](packet);
		}

		public function onLogin(packet: Packet): Boolean
		{
			Constants.playerId = packet.readUInt();
			Constants.sessionId = packet.readString();
			Global.map.usernames.players.add(new Username(Constants.playerId, packet.readString()));

			var now: Date = new Date();
			var serverTime: int = packet.readUInt();
			Constants.secondsPerUnit = Number(packet.readString());

			trace("Seconds per unit is " + Constants.secondsPerUnit);
			trace("Server Time is " + new Date(serverTime * 1000));
			var timeDelta: int = serverTime - int(now.time / 1000);
			trace("Delta is " + timeDelta);
			Global.map.setTimeDelta(timeDelta);

			// return whether it's a new player or not, which if it is we show the new city panel
			return packet.readByte() == 1;
		}

		public function readLoginInfo(packet: Packet): void
		{
			var cityCnt: int = packet.readUByte();
			for (var i: int = 0; i < cityCnt; i++)
			{
				var id: int = packet.readUInt();
				var name: String = packet.readString();
				var resources: LazyResources = new LazyResources(
				new LazyValue(packet.readInt(), packet.readInt(), packet.readInt(), packet.readInt(), packet.readUInt()),
				new LazyValue(packet.readInt(), packet.readInt(), 0, packet.readInt(), packet.readUInt()),
				new LazyValue(packet.readInt(), packet.readInt(), 0, packet.readInt(), packet.readUInt()),
				new LazyValue(packet.readInt(), packet.readInt(), 0, packet.readInt(), packet.readUInt()),
				new LazyValue(packet.readInt(), packet.readInt(), 0, packet.readInt(), packet.readUInt())
				);

				var radius: int = packet.readUByte();

				var city: City = new City(id, name, radius, resources);

				// Add the name of this city to the list of city names
				Global.map.usernames.cities.add(new Username(id, name));

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
					var objLabor: int = 0;
					if (ObjectFactory.getClassType(objType) == ObjectFactory.TYPE_STRUCTURE)
					objLabor = packet.readUByte();

					var cityObj: CityObject = new CityObject(city, objId, objType, objLvl, objX, objY, objLabor);

					var technologyCount: int = packet.readUShort();
					for (k = 0; k < technologyCount; k++)
					cityObj.techManager.add(new TechnologyStats(TechnologyFactory.getPrototype(packet.readUInt(), packet.readUByte()), EffectPrototype.LOCATION_OBJECT, objId));

					city.objects.add(cityObj, false);
				}

				var troopCnt: int = packet.readUByte();
				for (var troopI: int = 0; troopI < troopCnt; troopI++)
				{
					var troop: TroopStub = mapComm.Troop.readTroop(packet)
					city.troops.add(troop, false);
				}
				city.troops.sort();

				city.objects.sort();

				var templateCount: int = packet.readUShort();
				for (j = 0; j < templateCount; j++)
				city.template.add(new UnitTemplate(packet.readUShort(), packet.readUByte()));

				city.template.sort();

				Global.map.cities.add(city);
			}
		}
	}
}


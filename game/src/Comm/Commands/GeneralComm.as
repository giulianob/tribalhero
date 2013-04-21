package src.Comm.Commands {
	import flash.events.Event;
	import mx.formatters.DateFormatter;
	import org.aswing.AssetIcon;
	import src.Comm.*;
	import src.Constants;
	import src.Global;
	import src.UI.Dialog.CmdLineViewer;
	import src.Util.Util;
	import src.Map.*;
	import src.Objects.*;
	import src.Objects.Prototypes.*;
	import src.Objects.Factories.*;
	import src.Objects.Actions.*;
	import src.Objects.Troop.*;
	import src.UI.Components.ScreenMessages.BuiltInMessages;
	import src.UI.Components.ScreenMessages.ScreenMessageItem;
	import src.UI.Dialog.InfoDialog;
	import src.UI.Dialog.TribeProfileDialog;
	import src.UI.Components.ScreenMessages.BuiltInMessages;
	
	public class GeneralComm {

		private var mapComm: MapComm;
		private var session: Session;		

		public function GeneralComm(mapComm: MapComm) {
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
				case Commands.MESSAGE_BOX:
					onMessageBox(e.packet);
					break;
				case Commands.CHAT:
					onChatMessage(e.packet);
					break;
				case Commands.SYSTEM_CHAT:
					onChatSystemMessage(e.packet);
					break;
				
			}
		}				
		
		private function onMessageBox(packet: Packet): void
		{
			InfoDialog.showMessageDialog("Important Information", packet.readString());
		}
		
		private function onChatMessage(packet: Packet): void
		{
			var type: int = packet.readUByte();
			var achievements: * = {
				gold: packet.readUByte(),
				silver: packet.readUByte(),
				bronze: packet.readUByte()
			};
            var distinguish: Boolean = packet.readUByte() == 1 ? true : false;
			var playerId: int = packet.readUInt();
			var playerName: String = packet.readString();
			var message: String = packet.readString();		
			
			Global.gameContainer.cmdLine.logChat(type, playerId, playerName, achievements, distinguish, message);					
		}
		
		private function onChatSystemMessage(packet: Packet): void
		{
			var messageId: String = packet.readString();
			var paramsCount: int = packet.readUByte();
			var params: Array = new Array();
			for (var i: int = 0; i < paramsCount; i++) {
				params.push(packet.readString());
			}
			
			Global.gameContainer.cmdLine.logSystem(messageId, params);
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
            Constants.motd = packet.readString();
			Constants.playerId = packet.readUInt();
			Constants.admin = packet.readByte() == 1;
			Constants.sessionId = packet.readString();			
			Constants.playerName = packet.readString();			
			Constants.newbieProtectionSeconds = packet.readInt();
			Constants.signupTime = new Date(packet.readUInt() * 1000);
			Constants.tribeIncoming = packet.readInt();
			Constants.tribeAssignment = packet.readShort();

			var now: Date = new Date();
			var serverTime: int = packet.readUInt();
			Constants.secondsPerUnit = Number(packet.readString());

			Util.log("Seconds per unit is " + Constants.secondsPerUnit);
			Util.log("Server Time is " + new Date(serverTime * 1000));
			var timeDelta: int = serverTime - int(now.time / 1000);
			Util.log("Delta is " + timeDelta);
			Constants.timeDelta = timeDelta;			

			// return whether it's a new player or not, which if it is we show the new city panel
			return packet.readByte() == 1;
		}

		public function readLoginInfo(packet: Packet): void
		{
			// Tribe info
			Constants.tribe.id = packet.readUInt();
			Constants.tribeInviteId = packet.readUInt();
			Constants.tribe.rank = packet.readUByte();
			Global.gameContainer.tribeNotificationIcon.visible = Constants.tribeInviteId > 0;			
			
			var tribeName: String = packet.readString();
			if (Constants.tribe.isInTribe()) {
				Global.map.usernames.tribes.add(new Username(Constants.tribe.id, tribeName));

				TribeComm.readTribeRanks(packet);
			}
				
			// Cities
			var cityCnt: int = packet.readUByte();
			for (var i: int = 0; i < cityCnt; i++)			
				readCity(packet);			
		}
		
		public function readCity(packet: Packet) : City {
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

			var attackPoint: int = packet.readInt();
			var defensePoint: int = packet.readInt();
			
			var cityValue: int = packet.readUShort();
			
			var ap: Number = packet.readFloat();

			var inBattle: Boolean = packet.readByte() == 1;
			
			var hideNewUnits: Boolean = packet.readByte() == 1;

			var city: City = new City(id, name, radius, resources, attackPoint, defensePoint, cityValue, inBattle, hideNewUnits, ap);

			// Add the name of this city to the list of city names
			Global.map.usernames.cities.add(new Username(id, name));

			//Current Actions
			var currentActionCount: int = packet.readUByte();
			for (var k: int = 0; k < currentActionCount; k++) {

				var workerId: int = packet.readUInt();

				if (packet.readUByte() == 0) city.currentActions.add(new CurrentPassiveAction(workerId, packet.readUInt(), packet.readUShort(), packet.readString(), packet.readUInt(), packet.readUInt()), false);
				else city.currentActions.add(new CurrentActiveAction(workerId, packet.readUInt(), packet.readInt(), packet.readUByte(), packet.readUShort(), packet.readUInt(), packet.readUInt()), false);
			}
			city.currentActions.sort();

			//Notifications
			var notificationsCnt: int = packet.readUShort();
			for (k = 0; k < notificationsCnt; k++)
			{
				var notification: Notification = new Notification(city.id, packet.readUInt(), packet.readUInt(), packet.readUInt(), packet.readUShort(), packet.readUInt(), packet.readUInt());
				city.notifications.add(notification, false);
			}
			city.notifications.sort();
			
			//References
			var referencesCnt: int = packet.readUShort();
			for (k = 0; k < referencesCnt; k++) {
				var reference: CurrentActionReference = new CurrentActionReference(id, packet.readUShort(), packet.readUInt(), packet.readUInt());
				city.references.add(reference, false);
			}
			city.references.sort();

			//Structures
			var structCnt: int = packet.readUShort();

			for (var j: int = 0; j < structCnt; j++)
			{
				var regionId: int = packet.readUShort();
			
				var cityObj: CityObject = mapComm.City.readCityObject(packet, regionId, city);

				var technologyCount: int = packet.readUShort();
				for (k = 0; k < technologyCount; k++)
				cityObj.techManager.add(new TechnologyStats(TechnologyFactory.getPrototype(packet.readUInt(), packet.readUByte()), EffectPrototype.LOCATION_OBJECT, cityObj.objectId));

				city.objects.add(cityObj, false);
			}

			// Troop objects
			var troopCnt: int = packet.readUShort();

			for (j = 0; j < troopCnt; j++)
			{
				regionId = packet.readUShort();

				cityObj = mapComm.City.readCityObject(packet, regionId, city);
				
				city.objects.add(cityObj, false);
			}
			city.objects.sort();
			
			// Troop stubs
			troopCnt = packet.readUByte();
			for (var troopI: int = 0; troopI < troopCnt; troopI++)
			{
				var troop: TroopStub = mapComm.Troop.readTroop(packet);
				city.troops.add(troop, false);
			}
			city.troops.sort();				

			// Troop template
			var templateCount: int = packet.readUShort();
			for (j = 0; j < templateCount; j++) city.template.add(new UnitTemplate(packet.readUShort(), packet.readUByte()));
			city.template.sort();

			// Add city to player's cities
			Global.map.cities.add(city);
			
			// Show any messages
			BuiltInMessages.processAll(city);			
			
			return city;
		}
		
		public function sendCommand(command: String, callback: Function) : void {			
			var packet: Packet = new Packet();
			packet.cmd = Commands.CMD_LINE;
			packet.writeString(command);

			session.write(packet, onReceiveCommandResponse, [callback, 0]);
		}
		
		public function sendChat(type: int, message: String, callback: Function) : void {			
			var packet: Packet = new Packet();
			packet.cmd = Commands.CHAT;
			packet.writeByte(type);
			packet.writeString(message);

			session.write(packet, onReceiveCommandResponse, [callback, type]);
		}		

		private function onReceiveCommandResponse(packet: Packet, custom: *) : void {
			var callback: Function = custom[0];
			var type: int = custom[1];

			if ((packet.option & Packet.OPTIONS_FAILED) == Packet.OPTIONS_FAILED) {
				callback(GameError.getMessage(packet.readUInt()), type);
				return;
			}

			if (packet.hasData()) {
				callback(packet.readString(), type);
			}
		}
		
		public function autoCompleteCity(name: String, callback: Function) : void {
			var autocompleteLoader: GameURLLoader = new GameURLLoader();
			autocompleteLoader.addEventListener(Event.COMPLETE, function(e: Event): void {
				var data: Object;
				try
				{
					data = autocompleteLoader.getDataAsObject();
				}
				catch (e: Error) {					
					return;
				}
				
				callback(data, name);
			});
			
			autocompleteLoader.load("/cities/autocomplete", [ { key: "name", value: name }], true, false);
		}		
		
		public function autoCompleteStronghold(name: String, callback: Function) : void {
			var autocompleteLoader: GameURLLoader = new GameURLLoader();
			autocompleteLoader.addEventListener(Event.COMPLETE, function(e: Event): void {
				var data: Object;
				try
				{
					data = autocompleteLoader.getDataAsObject();
				}
				catch (e: Error) {					
					return;
				}
				
				callback(data, name);
			});
			
			autocompleteLoader.load("/strongholds/autocomplete", [ { key: "name", value: name }], true, false);
		}				
		
		public function autoCompletePlayer(name: String, callback: Function) : void {
			var autocompleteLoader: GameURLLoader = new GameURLLoader();
			autocompleteLoader.addEventListener(Event.COMPLETE, function(e: Event): void {
				var data: Object;
				try
				{
					data = autocompleteLoader.getDataAsObject();
				}
				catch (e: Error) {					
					return;
				}
				
				callback(data, name);
			});
			
			autocompleteLoader.load("/players/autocomplete", [ { key: "name", value: name }], true, false);
		}				
		
		public function viewProfileByType(type:String, id:int, callback:Function = null):void
		{
			var packet:Packet = new Packet();
			packet.cmd = Commands.PROFILE_BY_TYPE;
			packet.writeString(type);
			packet.writeUInt(id);
			
			mapComm.showLoading();
			
			switch (type.toLowerCase()) {
				case 'city':
					session.write(packet, mapComm.City.onReceivePlayerProfile, { callback: callback } );
					break;
				case 'stronghold':
					session.write(packet, mapComm.Stronghold.onReceiveStrongholdProfile, { callback: callback } );
					break;
				default:
					throw new Error("Unknown owner type while getting profile");
			}			
		}		
		
		public function readLocation(packet:Packet):*
		{
			var targetType: int = packet.readInt();
			
			switch (targetType) {
				case -1:
					return null;
				case Location.CITY:
					return {
						type: targetType,
						playerId: packet.readUInt(),
						cityId: packet.readUInt(),
						playerName: packet.readString(),
						cityName: packet.readString()
					};
				case Location.STRONGHOLD:
					return { 
						type: targetType,
						strongholdId: packet.readUInt(),
						strongholdName: packet.readString(),
						tribeId: packet.readUInt(),
						tribeName: packet.readString()
					};
				default:
					new Error("Unknown location type " + targetType);					
			}
		}
	}
}


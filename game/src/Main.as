package src
{
	import fl.lang.Locale;
	import flash.display.MovieClip;
	import flash.display.StageAlign;
	import flash.display.StageScaleMode;
	import flash.events.*;
	import flash.net.URLLoader;
	import flash.net.URLRequest;
	import flash.ui.ContextMenu;
	import flash.ui.ContextMenuItem;
	import org.aswing.skinbuilder.orange.*;
	import src.Map.*;
	import src.UI.Dialog.InfoDialog;
	import src.UI.Dialog.InitialCityDialog;
	import src.UI.Dialog.LoginDialog;
	import src.UI.LookAndFeel.GameLookAndFeel;
	import src.Util.*;
	import src.Comm.*;
	import src.Objects.Factories.*;
	import org.aswing.*;

	public class Main extends MovieClip
	{
		private var importObjects: ImportObjects;

		private var gameContainer: GameContainer;

		private var map:Map;
		private var miniMap: MiniMap;
		private var frameCounter:FPSCounter;
		public var packetCounter:GeneralCounter;
		private var session:TcpSession;
		private var password: String;
		private var parms: Object;

		private var loginDialog: LoginDialog;

		private var pnlLoading: InfoDialog;

		public function Main()
		{
			addEventListener(Event.ADDED_TO_STAGE, init);
		}

		public function init(e: Event = null) : void {
			removeEventListener(Event.ADDED_TO_STAGE, init);

			//Init ASWING
			AsWingManager.initAsStandard(stage);
			UIManager.setLookAndFeel(new GameLookAndFeel());

			//Init stage options
			stage.stageFocusRect = false;
			stage.scaleMode = StageScaleMode.NO_SCALE;
			stage.align = StageAlign.TOP_LEFT;

			Global.main = this;

			//Init right context menu for debugging
			if (Constants.debug > 0) {
				var fm_menu:ContextMenu = new ContextMenu();
				var dump:ContextMenuItem = new ContextMenuItem("Dump stage");
				dump.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, function(e:Event):void { Util.dumpDisplayObject(stage); } );
				var dumpRegionQueryInfo:ContextMenuItem = new ContextMenuItem("Dump region query info");
				dumpRegionQueryInfo.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, function(e:Event):void {
					if (!Global.map) return;
					trace("Pending regions:" + Util.implode(',', Global.map.pendingRegions));
				} );
				fm_menu.customItems.push(dump);
				fm_menu.customItems.push(dumpRegionQueryInfo);
				contextMenu = fm_menu;
			}

			//Flash params
			parms = loaderInfo.parameters;

			//GameContainer
			Global.gameContainer = gameContainer = new GameContainer();
			addChild(gameContainer);

			//Packet Counter
			if (Constants.debug > 0) {
				packetCounter = new GeneralCounter("pkts");
				packetCounter.y = Constants.screenH - 64;
				addChild(packetCounter);
			}

			//Define login type and perform login action
			if (parms.hostname)
			{
				Constants.loginKey = parms.lsessid;
				Constants.hostname = parms.hostname;
				loadLanguages(Constants.hostname);
			}
			else
			{
				showLoginDialog();
			}
		}

		private function loadLanguages(domain: String):void
		{
			pnlLoading = InfoDialog.showMessageDialog("Loading", "Launching the game...", null, null, true, false, 0);

			if (Constants.webVersion)
			{
				Locale.addXMLPath(Constants.defLang, "http://"+Constants.hostname+":8085/Game_" + Constants.defLang + ".xml");
			}
			else
			{
				Locale.addXMLPath(Constants.defLang, "en/Game_" + Constants.defLang + ".xml");
			}

			Locale.setDefaultLang(Constants.defLang);
			Locale.setLoadCallback(langLoaded);

			Locale.loadLanguageXML(Constants.defLang);
		}

		public function langLoaded(success: Boolean):void
		{
			if (Constants.queryData)
			{
				var loader: URLLoader = new URLLoader();
				loader.addEventListener(Event.COMPLETE, onReceiveXML);
				loader.load(new URLRequest("http://"+Constants.hostname+":8085/data.xml"));
			}
			else
			doConnect();
		}

		public function doConnect():void
		{
			session = new TcpSession();
			session.setConnect(onConnected);
			session.setDisconnect(onDisconnected);
			session.setLogin(onLogin);
			session.setSecurityErrorCallback(onSecurityError);
			session.connect(Constants.hostname);
		}

		public function showLoginDialog():void
		{
			gameContainer.closeAllFrames();
			loginDialog = new LoginDialog(onConnect);
			loginDialog.show();
		}

		public function onConnect(sender: LoginDialog):void
		{
			Constants.username = sender.getTxtUsername().getText();
			password = sender.getTxtPassword().getText();
			Constants.hostname = sender.getTxtAddress().getText();

			loadLanguages(Constants.hostname);
		}

		public function onSecurityError(event: SecurityErrorEvent):void
		{
			//if (pnlLoading) pnlLoading.getFrame().dispose();

			//InfoDialog.showMessageDialog("Security Error", event.toString());
		}

		public function onDisconnected(event: Event):void
		{
			gameContainer.dispose();

			Global.mapComm = null;
			Global.map = null;
			session = null;

			if (parms.hostname)
			{
				InfoDialog.showMessageDialog("Connection Lost", "Connection to Server Lost. Refresh the page to rejoin the battle.", null, null, true, false);
			}
			else
			{
				InfoDialog.showMessageDialog("Connection Lost", "Connection to server lost", function(result: int):void { if (!parms.hostname) showLoginDialog(); } );
			}
		}

		public function onConnected(event: Event, connected: Boolean):void
		{
			if (pnlLoading) pnlLoading.getFrame().dispose();

			if (!connected)
			{
				InfoDialog.showMessageDialog("Error", "Connection failed");
			}
			else
			{
				Global.mapComm = new MapComm(session);
				Global.map = map = new Map();
				miniMap = new MiniMap(Constants.miniMapScreenW, Constants.miniMapScreenH);

				if (Constants.loginKey)
				session.login(Constants.loginKey);
				else
				session.login(Constants.username, password);
			}

			password = '';
		}

		public function onLogin(packet: Packet):void
		{
			if ((packet.option & Packet.OPTIONS_FAILED) == Packet.OPTIONS_FAILED)
			{
				password = '';
				InfoDialog.showMessageDialog("Error", "Username or password was incorrect");
				return;
			}

			if (loginDialog != null) loginDialog.getFrame().dispose();

			var newPlayer: Boolean = Global.mapComm.Login.onLogin(packet);

			if (!newPlayer) {
				completeLogin(packet);
			}
			else {
				// Need to make the createInitialCity static and pass in the session
				var createCityDialog: InitialCityDialog = new InitialCityDialog(function(sender: InitialCityDialog): void {
					Global.mapComm.Login.createInitialCity(sender.getCityName(), completeLogin);
				});

				createCityDialog.show();
			}
		}

		public function onReceiveXML(e: Event):void
		{
			var str: String = e.target.data;

			Constants.objData = XML(str);

			doConnect();
		}

		public function completeLogin(packet: Packet):void
		{
			EffectReqFactory.init(map, Constants.objData);
			PropertyFactory.init(map, Constants.objData);
			StructureFactory.init(map, Constants.objData);
			TechnologyFactory.init(map, Constants.objData);
			UnitFactory.init(map, Constants.objData);
			WorkerFactory.init(map, Constants.objData);
			ObjectFactory.init(map, Constants.objData);

			gameContainer.show();
			Global.mapComm.Login.readLoginInfo(packet);
			gameContainer.setMap(map, miniMap);

			if (Constants.debug > 0) {
				if (frameCounter)
				removeChild(frameCounter);

				frameCounter = new FPSCounter();
				frameCounter.y = Constants.screenH - 32;
				addChild(frameCounter);
			}
		}

		public function onReceive(packet: Packet):void
		{
			if (Constants.debug >= 2)
			{
				trace("Received packet to main processor");
				trace(packet);
			}
		}

		private function resizeHandler(event:Event):void {
			trace("resizeHandler: " + event);
			trace("stageWidth: " + stage.stageWidth + " stageHeight: " + stage.stageHeight);
		}
	}
}


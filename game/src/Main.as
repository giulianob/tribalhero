package src
{
	import flash.display.Sprite;
	import flash.display.StageScaleMode;
	import flash.filters.DropShadowFilter;
	import org.aswing.plaf.ArrayUIResource;
	import org.aswing.plaf.ASColorUIResource;
	import org.aswing.plaf.ASFontUIResource;
	import org.aswing.plaf.basic.BasicLookAndFeel;
	import org.aswing.plaf.UIStyleTune;
	import org.flashdevelop.utils.FlashConnect;
	import fl.lang.Locale;
	import flash.display.DisplayObject;
	import flash.display.MovieClip;
	import flash.events.*;
	import flash.geom.Point;
	import flash.net.URLLoader;
	import flash.net.URLRequest;
	import flash.system.Security;
	import flash.text.FontStyle;
	import flash.text.FontType;
	import flash.text.StyleSheet;
	import flash.text.TextField;
	import flash.text.TextFormat;
	import flash.ui.ContextMenu;
	import flash.ui.ContextMenuItem;
	import src.Map.*;
	import src.UI.Dialog.Dialog;
	import src.UI.Dialog.InfoDialog;
	import src.UI.Dialog.LoginDialog;
	import src.UI.GameLookAndFeel;
	import src.UI.PaintBox;
	import src.Util.*;
	import src.Comm.*;
	import src.Objects.Factories.*;
	import org.aswing.*;	
	
	public class Main extends MovieClip
	{
		private var importObjects: ImportObjects;
		
		private var gameContainer: GameContainer;
		private var bgImage: MovieClip;
		
		private var map:Map;
		private var miniMap: MiniMap;
		private var mapComm: MapComm;
		private var frameCounter:FPSCounter;		
		public var packetCounter:GeneralCounter;
		private var session:TcpSession;
		private static var username: String = "123";
		private var password: String;
		private var hostname: String;
		private var parms: Object;
		
		private var loginDialog: LoginDialog;
		
		private var pnlLoading: InfoDialog;
		
		public function Main()
		{
			//Init ASWING
			AsWingManager.initAsStandard(stage);
			UIManager.setLookAndFeel(new GameLookAndFeel());
			
			Global.main = this;
			
			//Init right context menu for debugging
			var fm_menu:ContextMenu = new ContextMenu();
			var dump:ContextMenuItem = new ContextMenuItem("Dump stage");
			dump.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, function(e:Event):void { Util.dumpDisplayObject(stage); } );
			fm_menu.customItems.push(dump);
			contextMenu = fm_menu;						
			
			//Init Background
			bgImage = new IntroBackground(); 			
			addChild(bgImage);
			
			//Flash params
			parms = loaderInfo.parameters;
			
			//GameContainer
			Global.gameContainer = gameContainer = new GameContainer();
			addChild(gameContainer);									
		
			//Packet Counter
			packetCounter = new GeneralCounter("pkts");
			packetCounter.y = Constants.screenH - 64;
			addChild(packetCounter);									
			
			//Define login type and perform login action
			if (parms.hostname)			
			{
				hostname = parms.hostname;
				loadLanguages(hostname);
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
				Locale.addXMLPath(Constants.defLang, "http://"+hostname+"/Game_" + Constants.defLang + ".xml");			
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
				loader.load(new URLRequest("http://"+hostname+":8085/data.xml"));
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
			session.connect(hostname);						
		}
		
		public function showLoginDialog():void
		{
			loginDialog = new LoginDialog(onConnect);
			loginDialog.show();
		}
		
		public function onConnect(sender: LoginDialog):void
		{			
			username = sender.getTxtUsername().getText();
			password = sender.getTxtPassword().getText();
			hostname = sender.getTxtAddress().getText();
			
			loadLanguages(hostname);
		}
		
		public function onSecurityError(event: SecurityErrorEvent):void
		{
			if (pnlLoading) pnlLoading.getFrame().dispose();
			
			InfoDialog.showMessageDialog("Security Error", event.toString());
		}
		
		public function onDisconnected(event: Event):void
		{									
			gameContainer.dispose();
			
			if (parms.hostname)
			{
				InfoDialog.showMessageDialog("Connection Lost", "Connection to Server Lost. Refresh the page to rejoin the battle.");
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
				if (parms.lsessid)
					session.login(parms.lsessid);
				else
					session.login(username, password);				
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
			
			gameContainer.show();
						
			map = new Map(gameContainer.camera);
			mapComm = new MapComm(map, session);
			map.init(mapComm);
			
			miniMap = new MiniMap(map);
			
			completeLogin(packet);
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
			
			mapComm.Login.onLogin(packet);
			gameContainer.setMap(map, miniMap);
			map.parseRegions();
			
			if (frameCounter)
				removeChild(frameCounter);
				
			frameCounter = new FPSCounter();
			frameCounter.y = Constants.screenH - 32;
			addChild(frameCounter);							
		}
		
		public function onReceive(packet: Packet):void
		{
			if (Constants.debug >= 2)
			{
				trace("Received packet to main processor");
				trace(packet);
			}
		}
	}
}
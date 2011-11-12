﻿package src {
	import flash.display.*;
	import flash.events.*;
	import flash.geom.*;
	import flash.net.*;
	import flash.ui.*;
	import flash.utils.*;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.colorchooser.*;
	import org.aswing.event.*;
	import org.aswing.ext.*;
	import org.aswing.geom.*;
	import src.Components.*;
	import src.Map.*;
	import src.Objects.*;
	import src.Objects.Effects.*;
	import src.UI.*;
	import src.UI.Components.*;
	import src.UI.Components.ScreenMessages.*;
	import src.UI.Dialog.*;
	import src.Util.*;


	public class GameContainer extends GameContainer_base {

		//Popup menu
		private var menu: JPopupMenu;
		private var menuDummyOverlay: Component;

		//City list
		private var lstCities: JComboBox;

		//Currently selected sidebar
		private var sidebar: GameJSidebar;

		//Container for sidebar
		private var sidebarHolder: Sprite;

		public var map: Map;
		public var miniMap: MiniMap;				
		private var minimapRefreshTimer: Timer = new Timer(500000, 0);

		//Holds any overlay. Overlays are used for different cursor types.
		private var mapOverlay: Sprite;
		private var mapOverlayTarget: Sprite;

		//HUD resources container
		private var resourcesContainer: ResourcesContainer;

		//Container for messages that can be set by different sidebars
		public var message: MessageContainer = new MessageContainer();

		//Holds all currently open aswing frames
		private var frames: Array = new Array();

		public var selectedCity: City;
		public var camera: Camera = new Camera(0, 0);

		//Resources timer that fires every second
		public var resourcesTimer: Timer = new Timer(1000);

		//Timer to load unread message count
		public var messageTimer: MessageTimer;

		//On screen message component
		public var screenMessage: ScreenMessagePanel;

		//Holds the tools above the minimap
		public var minimapTools: MinimapToolsContainer = new MinimapToolsContainer();
		public var minimapZoomed: Boolean = false;
		private var minimapZoomTooltip: SimpleTooltip;

		//Handles fancy auto resizing
		public var resizeManager: ResizeManager;

		// Game bar bg. Can't be in Flash because of scale9 issue
		private var barBg: DisplayObject;

		// Command line
		private var cmdLine: CmdLineViewer;
		
		// Holds currently pressed keys
		private var pressedKeys:Object = { };
		
		// Shows built in messages on the game screen
		private var builtInMessages: BuiltInMessages;

		public function GameContainer()
		{							
			// Here we create a dummy ASWing component and stick it over the menu button because the popup requires it
			menuDummyOverlay = new Component();
			menuDummyOverlay.setLocationXY(btnMenu.x, btnMenu.y);			
			
			// Create and position the city list
			lstCities = new JComboBox();			
			lstCities.setModel(new VectorListModel());
			lstCities.addEventListener(InteractiveEvent.SELECTION_CHANGED, onChangeCitySelection);
			lstCities.setSize(new IntDimension(128, 22));
			lstCities.setLocation(new IntPoint(40, 12));
			addChild(lstCities);

			// Create barBg
			var barBgClass: Class = UIManager.getDefaults().get("GameMenu.bar");
			barBg = new barBgClass() as DisplayObject;
			addChildAt(barBg, 1);

			// Hide the menu bubbles
			tribeInviteRequest.visible = false;
			tribeInviteRequest.mouseChildren = false;
			tribeInviteRequest.mouseEnabled = false;
			
			txtUnreadMessages.visible = false;
			txtUnreadMessages.mouseChildren = false;
			txtUnreadMessages.mouseEnabled = false;

			txtUnreadReports.visible = false;
			txtUnreadReports.mouseChildren = false;
			txtUnreadReports.mouseEnabled = false;
			
			txtIncoming.visible = false;
			txtIncoming.mouseChildren = false;
			txtIncoming.mouseEnabled = false;

			// Add key down listener to stage
			addEventListener(Event.ADDED_TO_STAGE, function(e: Event):void {
				stage.addEventListener(KeyboardEvent.KEY_DOWN, eventKeyDown);
				stage.addEventListener(KeyboardEvent.KEY_UP, eventKeyUp);
			});

			// Hide game container for now
			visible = false;

			// Hide the connecting chains for the sidebar (this is just a graphic)
			chains.visible = false;

			// Disable mouse for coordinates
			minimapTools.txtCoords.mouseEnabled = false;

			// Set up tooltips
			minimapZoomTooltip = new SimpleTooltip(minimapTools.btnMinimapZoom);
			minimapTools.btnMinimapZoom.addEventListener(MouseEvent.CLICK, onZoomIntoMinimap);

			new SimpleTooltip(minimapTools.btnGoToCoords, "Find cities, players, and coordinates");
			minimapTools.btnGoToCoords.addEventListener(MouseEvent.CLICK, onGoToCoords);
			
			new SimpleTooltip(minimapTools.btnFeedback, "Send Feedback");
			minimapTools.btnFeedback.addEventListener(MouseEvent.CLICK, onSendFeedback);
			
			new SimpleTooltip(minimapTools.btnZoomIn, "Zoom In");
			minimapTools.btnZoomIn.addEventListener(MouseEvent.CLICK, onZoomIn);
			
			new SimpleTooltip(minimapTools.btnZoomOut, "Zoom Out");
			minimapTools.btnZoomOut.addEventListener(MouseEvent.CLICK, onZoomOut);

			new SimpleTooltip(btnGoToCity, "Go to city");
			btnGoToCity.addEventListener(MouseEvent.CLICK, onGoToCity);

			new SimpleTooltip(btnReports, "View battle reports");
			btnReports.addEventListener(MouseEvent.CLICK, onViewReports);

			new SimpleTooltip(btnMessages, "View messages");
			btnMessages.addEventListener(MouseEvent.CLICK, onViewMessages);

			new SimpleTooltip(btnRanking, "View world ranking");
			btnRanking.addEventListener(MouseEvent.CLICK, onViewRanking);

			new SimpleTooltip(btnCityInfo, "View city details");
			btnCityInfo.addEventListener(MouseEvent.CLICK, onViewCityInfo);

			new SimpleTooltip(btnCityTroops, "View unit movement");
			btnCityTroops.addEventListener(MouseEvent.CLICK, onViewCityTroops);
			
			new SimpleTooltip(btnTribe, "View tribe");
			btnTribe.addEventListener(MouseEvent.CLICK, onViewTribe);
			
			btnMenu.addEventListener(MouseEvent.CLICK, onMenuClick);		

			// Set up sidebar holder
			sidebarHolder = new Sprite();
			sidebarHolder.x = Constants.screenW - GameJSidebar.WIDTH - 15;
			sidebarHolder.y = 60;
			addChildAt(sidebarHolder, 1);

			// Set up minimap refresh timer
			minimapRefreshTimer.addEventListener(TimerEvent.TIMER, minimapRefresh);
			
			//Set up resources timer
			resourcesTimer.addEventListener(TimerEvent.TIMER, displayResources);			
		}

		public function onMenuClick(e: MouseEvent): void
		{
			if (!menu) return;
			
			menu.show(menuDummyOverlay, 0, btnMenu.height);
		}

		public function onLogoutClick(e: Event): void
		{
			navigateToURL(new URLRequest("http://" + Constants.mainWebsite + "/players/logout/session:" + Constants.sessionId), "_self");
		}
		
		public function onAccountOptionsClick(e: Event): void
		{
			navigateToURL(new URLRequest("http://" + Constants.mainWebsite + "/players/account"), "_blank");
		}		
		
		public function onHelpClick(e: Event): void
		{
			navigateToURL(new URLRequest("http://" + Constants.mainWebsite + "/database"), "_blank");
		}
		
		public function onForumsClick(e: Event): void
		{
			navigateToURL(new URLRequest("http://forums.tribalhero.com"), "_blank");
		}		

		public function onViewCityTroops(e: MouseEvent) :void
		{
			if (!selectedCity)
				return;

			var movementDialog: MovementDialog = new MovementDialog(selectedCity);
			movementDialog.show();
		}
		
		public function onViewTribe(e: MouseEvent) :void
		{			
			if (Constants.tribeId != 0) {				
				Global.mapComm.Tribe.viewTribeProfile(function(profileData: *): void {
					if (!profileData) 
						return;
					
					var dialog: TribeProfileDialog = new TribeProfileDialog(profileData);
					dialog.show();
				});
			}
			else if (Constants.tribeInviteId != 0) {
				var tribeInviteDialog: TribeInviteRequestDialog = new TribeInviteRequestDialog(function(sender: TribeInviteRequestDialog) : void {
					Global.mapComm.Tribe.invitationConfirm(sender.getResult());
					
					sender.getFrame().dispose();
				});				
				tribeInviteDialog.show();
			}
			else {
				var createTribeDialog: CreateTribeDialog = new CreateTribeDialog(function(sender: CreateTribeDialog) : void {
					Global.mapComm.Tribe.createTribe(sender.getTribeName());
					sender.getFrame().dispose();
				});
				createTribeDialog.show();
			}
		}

		public function onViewCityInfo(e: MouseEvent) :void
		{
			if (!selectedCity)
			return;

			var currentEventDialog: CityEventDialog = new CityEventDialog(selectedCity);
			currentEventDialog.show();
		}

		public function onViewRanking(e: MouseEvent) :void
		{
			if (!selectedCity)
			return;

			var rankingDialog: RankingDialog = new RankingDialog();
			rankingDialog.show();
		}

		public function onViewReports(e: MouseEvent):void
		{
			var battleReportDialog: BattleReportList = new BattleReportList();
			battleReportDialog.show(null, true, function(dialog: BattleReportList) : void {
				if (battleReportDialog.getRefreshOnClose()) {
					messageTimer.check();
				}
			});
		}

		public function onViewMessages(e: MouseEvent):void
		{
			var messagingDialog: MessagingDialog = new MessagingDialog();
			messagingDialog.show(null, true, function(dialog: MessagingDialog) : void {
				if (messagingDialog.getRefreshOnClose()) {
					messageTimer.check();
				}
			});
		}

		public function onGoToCity(e: Event) : void {
			if (selectedCity == null) return;

			var pt: Point = MapUtil.getScreenCoord(selectedCity.MainBuilding.x, selectedCity.MainBuilding.y);
			Global.gameContainer.map.camera.ScrollToCenter(pt.x, pt.y);
		}

		public function onGoToCoords(e: Event) : void {
			var goToDialog: GoToDialog = new GoToDialog();
			goToDialog.show();
		}
		
		public function onSendFeedback(e: Event) : void {
			navigateToURL(new URLRequest("http://" + Constants.mainWebsite + "/feedback"), "_blank");
		}
		
		public function onZoomIn(e: Event) : void {		
			if (camera.getZoomFactor() >= 0.99) return;
			var center: Point = camera.GetCenter();
			camera.setZoomFactor(Math.min(1, camera.getZoomFactor() + 0.1));
			map.scrollRate = 1 * camera.getZoomFactorOverOne();
			mapHolder.scaleX = mapHolder.scaleY = camera.getZoomFactor();
			miniMap.redraw();
			camera.ScrollToCenter(center.x, center.y);
		}		
		
		public function onZoomOut(e: Event) : void {
			if (camera.getZoomFactor() <= 0.61) return;
			var center: Point = camera.GetCenter();
			camera.setZoomFactor(Math.max(0.6, camera.getZoomFactor() - 0.1));
			map.scrollRate = 1 * camera.getZoomFactorOverOne();
			mapHolder.scaleX = mapHolder.scaleY = camera.getZoomFactor();			
			miniMap.redraw();
			camera.ScrollToCenter(center.x, center.y);
		}			

		public function onZoomIntoMinimap(e: Event):void {
			if (minimapZoomed == false) {
				Global.map.camera.cue();
			}
			else {
				Global.map.camera.goToCue();
			}

			zoomIntoMinimap(!minimapZoomed);
		}

		public function zoomIntoMinimap(zoom: Boolean, query: Boolean = true) : void {
			clearAllSelections();
			
			if (zoom) {
				screenMessage.setVisible(false);
				// We leave a bit of border incase the screen is smaller than the map size
				var width: int = Math.min(Constants.screenW - 60, Constants.miniMapLargeScreenW);
				var height: int = Math.min(Constants.screenH - 75, Constants.miniMapLargeScreenH);
				miniMap.resize(width, height);
				miniMap.x = Constants.miniMapLargeScreenX(width);
				miniMap.y = Constants.miniMapLargeScreenY(height);
				minimapZoomTooltip.setText("Minimize map");
				miniMap.setScreenRectHidden(true);
				map.disableMapQueries(true);
				map.scrollRate = 25;
				minimapTools.btnZoomIn.visible = false;
				minimapTools.btnZoomOut.visible = false;
				message.showMessage("Double click to go anywhere\nPress Escape to close this map");
			}
			else {
				screenMessage.setVisible(true);
				miniMap.resize(Constants.miniMapScreenW, Constants.miniMapScreenH);
				miniMap.x = Constants.miniMapScreenX(Constants.miniMapScreenW);
				miniMap.y = Constants.miniMapScreenY(Constants.miniMapScreenH);
				minimapZoomTooltip.setText("World view");
				miniMap.setScreenRectHidden(false);
				map.disableMapQueries(false);
				map.scrollRate = 1 * camera.getZoomFactorOverOne();
				minimapTools.btnZoomIn.visible = true;
				minimapTools.btnZoomOut.visible = true;
				message.hide();
			}

			minimapZoomed = zoom;
			if (query) {
				map.move(true);
			}

			alignMinimapTools();
		}

		public function eventKeyUp(event: KeyboardEvent):void
		{
			// clear key press
			delete pressedKeys[event.keyCode];
		}		
		
		public function isKeyDown(keyCode: int) : Boolean
		{
			return pressedKeys[keyCode];
		}
		
		public function eventKeyDown(e: KeyboardEvent):void
		{
			// Key down handler
			
			// end key down handler
			
			// Key Press Handler
			if(pressedKeys[e.keyCode]) return;
			pressedKeys[e.keyCode] = 1;
			
			// Escape key functions
			if (e.charCode == Keyboard.ESCAPE)
			{						
				// Unzoom map
				if (miniMap != null) zoomIntoMinimap(false, false);
				
				// Deselect objects
				clearAllSelections();
				
				// Close top most frame if possible
				closeTopmostFrame();
			}
			
			// Keys that should only apply if we are on the map w/o any dialogs open
			if (frames.length == 0) {
				
				// Moving around with arrow keys
				map.camera.beginMove();
				var keyScrollRate: int = minimapZoomed ? 1150 : 500 * map.camera.getZoomFactorOverOne();
				if (e.keyCode == Keyboard.LEFT) map.camera.MoveLeft(keyScrollRate);
				if (e.keyCode == Keyboard.RIGHT) map.camera.MoveRight(keyScrollRate);
				if (e.keyCode == Keyboard.UP) map.camera.MoveUp(keyScrollRate);
				if (e.keyCode == Keyboard.DOWN) map.camera.MoveDown(keyScrollRate);
				map.camera.endMove();
				
				// Zoom into minimap with +/- keys
				if (!minimapZoomed && !Util.textfieldHasFocus(stage)) {
					if (e.keyCode == 187 || e.keyCode == Keyboard.NUMPAD_ADD) onZoomIn(e);							
					if (e.keyCode == 189 || e.keyCode == Keyboard.NUMPAD_SUBTRACT) onZoomOut(e);
				}
			}
		}
		
		public function getSelectedCityIndex(): int
		{
			return lstCities.getSelectedIndex();
		}

		public function setMap(map: Map, miniMap: MiniMap):void
		{
			this.map = map;
			this.miniMap = miniMap;

			// Clear current city list
			(lstCities.getModel() as VectorListModel).clear();

			// Add map
			mapHolder.addChild(map);

			// Create map overlay
			this.mapOverlay = new MovieClip();
			this.mapOverlay.graphics.beginFill(0xCCFF00);
			this.mapOverlay.graphics.drawRect(0, 0, Constants.screenW, Constants.screenH);
			this.mapOverlay.visible = false;
			this.mapOverlay.mouseEnabled = false;
			this.mapOverlay.name = "Overlay";
			addChild(this.mapOverlay);

			// Populate city list
			for each (var city: City in map.cities.each()) {
				addCityToUI(city);
			}

			// Set a default city selection
			if (lstCities.getItemCount() > 0) {
				lstCities.setSelectedIndex(0);
				selectedCity = lstCities.getSelectedItem().city;
			}
			else {
				map.onMove();
			}

			//Show resources box
			resourcesContainer = new ResourcesContainer();
			displayResources();

			//Add minimap tools
			addChild(minimapTools);

			// Create and position command line if admin
			if (Constants.admin) {
				cmdLine = new CmdLineViewer();
				cmdLine.show();
				cmdLine.getFrame().hide();
				cmdLine.getFrame().setLocationXY(300, Constants.screenH - cmdLine.getFrame().getHeight() + 22);
			}

			// Add objects to resize manager
			resizeManager = new ResizeManager(stage);

			resizeManager.addObject(this.mapOverlay, ResizeManager.ANCHOR_RIGHT | ResizeManager.ANCHOR_TOP | ResizeManager.ANCHOR_LEFT | ResizeManager.ANCHOR_BOTTOM);
			resizeManager.addObject(sidebarHolder, ResizeManager.ANCHOR_RIGHT | ResizeManager.ANCHOR_TOP);
			resizeManager.addObject(barBg, ResizeManager.ANCHOR_RIGHT | ResizeManager.ANCHOR_LEFT);
			resizeManager.addObject(resourcesContainer, ResizeManager.ANCHOR_TOP | ResizeManager.ANCHOR_RIGHT);
			resizeManager.addObject(miniMap, ResizeManager.ANCHOR_LEFT | ResizeManager.ANCHOR_BOTTOM);
			resizeManager.addObject(minimapTools, ResizeManager.ANCHOR_LEFT | ResizeManager.ANCHOR_BOTTOM);
			resizeManager.addObject(chains, ResizeManager.ANCHOR_RIGHT | ResizeManager.ANCHOR_TOP);
			if (cmdLine) resizeManager.addObject(cmdLine, ResizeManager.ANCHOR_BOTTOM | ResizeManager.ANCHOR_LEFT);

			resizeManager.addEventListener(Event.RESIZE, map.onResize);
			resizeManager.addEventListener(Event.RESIZE, message.onResize);

			resizeManager.forceMove();

			// Scroll to city center
			if (selectedCity) {
				var pt: Point = MapUtil.getScreenCoord(selectedCity.MainBuilding.x, selectedCity.MainBuilding.y);
				src.Global.gameContainer.camera.ScrollToCenter(pt.x, pt.y);
			}

			//Set minimap position and initial state
			miniMap.addEventListener(MiniMap.NAVIGATE_TO_POINT, onMinimapNavigateToPoint);
			addChild(miniMap);
			zoomIntoMinimap(false, false);
		}

		public function show() : void {
			// Create popup menu now that we have all the player info
			menu = new JPopupMenu();
			menu.addMenuItem("Logged in as " + Constants.username);			
			menu.addMenuItem("Account Options").addActionListener(onAccountOptionsClick);
			menu.addMenuItem("Forums").addActionListener(onForumsClick);
			menu.addMenuItem("Help").addActionListener(onHelpClick);			
			menu.addMenuItem("Logout").addActionListener(onLogoutClick);

			// Reset camera pos
			camera.reset();

			// Close any previous open frames (Shouldnt really have any but just to be safe)
			closeAllFrames();

			// Set visible
			visible = true;

			// Create on screen message component, it'll auto show itself
			screenMessage = new ScreenMessagePanel(this);
			
			// Add menu overlay
			addChild(menuDummyOverlay);
			
			// Start timers
			minimapRefreshTimer.start();
			resourcesTimer.start();

			// Create message timer to check for new msgs
			messageTimer = new MessageTimer();
			messageTimer.start();
			
			// Show built in messages
			builtInMessages = new BuiltInMessages();
			builtInMessages.start();
		}

		public function dispose() : void {
			if (menu) {
				menu.dispose();
				menu = null;
				removeChild(menuDummyOverlay);
			}
			
			if (builtInMessages) {
				builtInMessages.stop();
			}
			
			if (resourcesTimer) {
				resourcesTimer.stop();												
			}
			
			if (resourcesContainer && resourcesContainer.getFrame())
				resourcesContainer.getFrame().dispose();			
			
			if (minimapRefreshTimer) {
				minimapRefreshTimer.stop();
			}

			if (messageTimer) {
				messageTimer.stop();
				messageTimer = null;
			}

			if (resizeManager) {
				resizeManager.removeAllObjects();
			}

			if (cmdLine) {
				cmdLine.getFrame().dispose();
			}

			message.hide();

			closeAllFrames();

			visible = false;

			if (resourcesContainer && resourcesContainer.getFrame()) {
				resourcesContainer.getFrame().dispose();
			}

			if (screenMessage) {
				screenMessage.dispose();
			}

			resourcesContainer = null;
			clearAllSelections();

			if (map) {
				resizeManager.removeEventListener(Event.RESIZE, map.onResize);
				resizeManager.removeEventListener(Event.RESIZE, message.onResize);
				miniMap.removeEventListener(MiniMap.NAVIGATE_TO_POINT, onMinimapNavigateToPoint);

				map.dispose();
				mapHolder.removeChild(map);
				removeChild(mapOverlay);
				removeChild(miniMap);
				removeChild(minimapTools);

				map = null;
				miniMap = null;
			}

			resizeManager = null;
		}
		
		public function addCityToUI(city: City): void {
			(lstCities.getModel() as VectorListModel).append( { id: city.id, city: city, toString: function() : String { return this.city.name; } } );
		}

		private function alignMinimapTools() : void {
			minimapTools.x = miniMap.x;
			minimapTools.y = miniMap.y - 3;
		}

		public function clearAllSelections() : void 
		{
			if (map)
				map.selectObject(null);
				
			setSidebar(null);
		}
		
		public function setSidebar(sidebar: GameJSidebar):void
		{			
			if (this.sidebar != null)
				this.sidebar.getFrame().dispose();			

			chains.visible = false;
			this.sidebar = sidebar;

			if (sidebar != null) {
				chains.visible = true;							
				sidebar.show(sidebarHolder);								
			}					
			
			stage.focus = map;
		}
		
		public function closeTopmostFrame(onlyIfClosable: Boolean = true) : void {
			if (frames.length == 0)
				return;
			
			var frame: JFrame = frames[frames.length-1] as JFrame;
			if (onlyIfClosable && !frame.isClosable())
				return;
			
			frame.dispose();
		}

		public function closeAllFrames(onlyClosableFrames: Boolean = false) : void {
			var framesCopy: Array = frames.concat();

			for (var i: int = framesCopy.length - 1; i >= 0; --i) {
				var frame: JFrame = framesCopy[i] as JFrame;
				if (onlyClosableFrames && !frame.isClosable())
					break;
				
				frame.dispose();
			}
		}
	
		public function closeAllFramesByType(type:Class, onlyClosableFrames: Boolean = false) : void {
			var framesCopy: Array = frames.concat();

			for (var i: int = framesCopy.length - 1; i >= 0; --i) {
				var frame: JFrame = framesCopy[i] as JFrame;
				if (onlyClosableFrames && !frame.isClosable())
					break;
				if(frame.getContentPane() is type)
					frame.dispose();
			}
		}

		public function showFrame(frame: JFrame):void {						
			if (map != null) {
				clearAllSelections();
				map.disableMouse(true);				
			}
				
			frames.push(frame);
			frame.addEventListener(PopupEvent.POPUP_CLOSED, onFrameClosing);
			frame.show();			
		}
		
		public function findDialog(type: Class): * {
			for (var i: int = frames.length - 1; i >= 0; i--) {
				if (frames[i].getContentPane() is type)
					return frames[i].getContentPane();
			}
			
			return null;
		}

		public function onFrameClosing(e: PopupEvent):void {
			var frame: JFrame = e.target as JFrame;
			if (frame == null) return;
			frame.removeEventListener(PopupEvent.POPUP_CLOSED, onFrameClosing);
			var index: int = frames.indexOf(frame);
			if (index == -1) Util.log("Closed a frame that did not call show through GameContainer");
			frames.splice(index, 1);
			if (frames.length == 0 && map != null) map.disableMouse(false);
		}

		public function minimapRefresh(e: Event = null):void {
			if (miniMap == null) return;
			
			miniMap.parseRegions(true);
		}
		
		public function displayResources(e: Event = null):void {
			if (!resourcesContainer) return;

			if (selectedCity == null)
			{
				if (resourcesContainer.getFrame())
					resourcesContainer.getFrame().dispose();

				return;
			}

			Global.gameContainer.selectedCity.dispatchEvent(new Event(City.RESOURCES_UPDATE));

			resourcesContainer.displayResources();
		}

		public function setOverlaySprite(object: Sprite):void
		{
			if (this.mapOverlayTarget != null)
			{
				this.mapOverlayTarget.hitArea = null;

				var disposeTmp: IDisposable = this.mapOverlayTarget as IDisposable;

				if (disposeTmp != null)
					disposeTmp.dispose();

				mapHolder.removeChild(this.mapOverlayTarget);
				this.mapOverlayTarget = null;
			}

			this.mapOverlayTarget = object;

			if (this.mapOverlayTarget != null)
			{
				mapHolder.addChild(this.mapOverlayTarget);
				this.mapOverlayTarget.hitArea = this.mapOverlay;
			}
		}

		public function selectCity(cityId: int) : void {
			if (Global.gameContainer.selectedCity.id == cityId) return;
			
			for (var i: int = 0; i < lstCities.getModel().getSize(); i++) {
				var item: * = lstCities.getModel().getElementAt(i);
				
				if (item.id == cityId) {
					lstCities.setSelectedIndex(i, true);
					
					setSidebar(null);
					selectedCity = lstCities.getSelectedItem().city;
					displayResources();
					break;
				}
			}
		}
		
		public function onChangeCitySelection(e: InteractiveEvent):void {			
			setSidebar(null);			
			
			selectedCity = null;			
			if (lstCities.getSelectedIndex() == -1) return;
			
			selectedCity = lstCities.getSelectedItem().city;
			displayResources();			
			onGoToCity(e);
			
			stage.focus = map;
		}

		private function onMinimapNavigateToPoint(e: MouseEvent) : void {
			if (minimapZoomed) {
				zoomIntoMinimap(false);
			}

			Global.map.camera.ScrollToCenter(e.localX, e.localY);
		}
	}

}


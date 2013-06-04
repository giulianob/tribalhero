package src {
    import com.greensock.TweenMax;
	import flash.display.*;
	import flash.events.*;
	import flash.geom.*;
	import flash.net.*;
	import flash.text.TextField;
	import flash.ui.*;
	import flash.utils.*;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.colorchooser.*;
	import org.aswing.event.*;
	import org.aswing.ext.*;
	import org.aswing.geom.*;
	import src.Map.*;
	import src.Objects.*;
	import src.Objects.Effects.*;
	import src.UI.*;
	import src.UI.Components.*;
	import src.UI.Components.ScreenMessages.*;
	import src.UI.Dialog.*;
	import src.UI.Tutorial.GameTutorial;
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
		
		//Container for cmd line
		private var cmdLineHolder: Sprite;

		public var map: Map;
		public var miniMap: MiniMap;				
		public var minimapHolder: Sprite;
		private var minimapRefreshTimer: Timer = new Timer(500000, 0);

		//Holds any overlay. Overlays are used for different cursor types.
		private var mapOverlay: Sprite;
		private var mapOverlayTarget: Sprite;

		//HUD resources container
		private var resourcesContainer: ResourcesContainer;

		//Container for messages that can be set by different sidebars
		public var message: MessageContainer = new MessageContainer();

		//Holds all currently open aswing frames
		public var frames: Array = [];

		public var selectedCity: City;
		public var camera: Camera = new Camera(0, 0);

		//Resources timer that fires every second
		public var resourcesTimer: Timer = new Timer(1000);

		//On screen message component
		public var screenMessage: ScreenMessagePanel;
        public var screenMessageHolder: Sprite;

		//Holds the tools above the minimap
		public var minimapTools: MinimapToolsContainer = new MinimapToolsContainer();
		public var minimapZoomed: Boolean = false;
		private var minimapZoomTooltip: SimpleTooltip;

		//Handles fancy auto resizing
		public var resizeManager: ResizeManager;

		// Game bar bg. Can't be in Flash because of scale9 issue
		private var barBg: DisplayObject;

		// Command line
		public var cmdLine: CmdLineViewer;
		
		// Holds currently pressed keys
		private var pressedKeys:Object = { };
		
		// Shows built in messages on the game screen
		private var builtInMessages: BuiltInMessages;
		
		// Game tutorial handler
		private var tutorial: GameTutorial;

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

			// Hide the menu bubbles
			tribeNotificationIcon.visible = false;
			tribeNotificationIcon.mouseChildren = false;
			tribeNotificationIcon.mouseEnabled = false;
			
			txtUnreadMessages.visible = false;
			txtUnreadMessages.mouseChildren = false;
			txtUnreadMessages.mouseEnabled = false;

			txtUnreadReports.visible = false;
			txtUnreadReports.mouseChildren = false;
			txtUnreadReports.mouseEnabled = false;			

			// Hide game container for now
			visible = false;

			// Hide the connecting chains for the sidebar (this is just a graphic)
			chains.visible = false;

			// Disable mouse for coordinates
			minimapTools.txtCoords.mouseEnabled = false;

			// Set up tooltips
			minimapZoomTooltip = new SimpleTooltip(minimapTools.btnMinimapZoom);
			minimapTools.btnMinimapZoom.addEventListener(MouseEvent.CLICK, onZoomIntoMinimap);

			new SimpleTooltip(minimapTools.btnGoToCoords, "Find...");
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

			// Set up holders
			sidebarHolder = new Sprite();
			sidebarHolder.x = Constants.screenW - GameJSidebar.WIDTH - 15;
			sidebarHolder.y = 60;			
			
			cmdLineHolder = new Sprite();				
			minimapHolder = new Sprite();
            screenMessageHolder = new Sprite();
            screenMessageHolder.mouseEnabled = false;
            screenMessageHolder.mouseChildren = false;
			
            addChild(screenMessageHolder);
			addChild(cmdLineHolder);								
			addChild(minimapHolder);				
			addChild(sidebarHolder);
			
			// Bar bg			
			var barBgClass: Class = UIManager.getDefaults().get("GameMenu.bar");
			barBg = new barBgClass() as DisplayObject;						
			addChildAt(barBg, 1);
			
			// Minimap tools
			minimapHolder.addChild(minimapTools);

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
			navigateToURL(new URLRequest(Constants.mainWebsite + "players/logout/session:" + Constants.sessionId), "_self");
		}
		
		public function onProfileClick(e: Event): void
		{
			Global.mapComm.City.viewPlayerProfile(Constants.playerId);
		}		

		public function onAccountOptionsClick(e: Event): void
		{
			navigateToURL(new URLRequest(Constants.mainWebsite + "players/account"), "_blank");
		}		
		
		public function onHelpClick(e: Event): void
		{
			navigateToURL(new URLRequest(Constants.mainWebsite + "database"), "_blank");
		}
		
		public function onWikiClick(e: Event): void
		{
			navigateToURL(new URLRequest("http://tribalhero.wikia.com"), "_blank");
		}		
		
		public function onForumsClick(e: Event): void
		{
			navigateToURL(new URLRequest("http://forums.tribalhero.com"), "_blank");
		}		

		public function onViewCityTroops(e: MouseEvent) :void
		{
			if (!selectedCity) {
				return;
			}
            
            var currentDialog: TroopsDialog = findDialog(TroopsDialog);
            
            if (currentDialog) {
                currentDialog.getFrame().dispose();
                return;
            }            

			new TroopsDialog(selectedCity).show();
		}
		
		public function onViewTribe(e: MouseEvent) :void
		{			
			if (Constants.tribe.isInTribe()) {				
				var currentTribeDialog: TribeProfileDialog = findDialog(TribeProfileDialog);
            
				if (currentTribeDialog) {
					currentTribeDialog.getFrame().dispose();
					return;
				}
				
				Global.mapComm.Tribe.viewTribeProfile(Constants.tribe.id);
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
			if (!selectedCity) return;

            var currentEventDialog: CityEventDialog = findDialog(CityEventDialog);
            
            if (currentEventDialog) {
                currentEventDialog.getFrame().dispose();
                return;
            }
            
			new CityEventDialog(selectedCity).show(null, false);
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
					Global.mapComm.Messaging.refreshUnreadCounts();
				}
			});
		}

		public function onViewMessages(e: MouseEvent):void
		{
			var messagingDialog: MessagingDialog = new MessagingDialog();
			messagingDialog.show(null, true, function(dialog: MessagingDialog) : void {
				if (messagingDialog.getRefreshOnClose()) {
					Global.mapComm.Messaging.refreshUnreadCounts();
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
			navigateToURL(new URLRequest(Constants.mainWebsite + "feedback"), "_blank");
		}
		
		public function onZoomIn(e: Event) : void {		
			if (camera.getZoomFactor() >= 0.99 || minimapZoomed) return;
			var center: Point = camera.GetCenter();
			camera.setZoomFactor(Math.min(1, camera.getZoomFactor() + 0.1));
			map.scrollRate = 1 * camera.getZoomFactorOverOne();
			mapHolder.scaleX = mapHolder.scaleY = camera.getZoomFactor();
			miniMap.redraw();
			camera.ScrollToCenter(center.x, center.y);
		}		
		
		public function onZoomOut(e: Event) : void {
			if (camera.getZoomFactor() <= 0.61 || minimapZoomed) return;
			var center: Point = camera.GetCenter();
			camera.setZoomFactor(Math.max(0.6, camera.getZoomFactor() - 0.1));
			map.scrollRate = 1 * camera.getZoomFactorOverOne();
			mapHolder.scaleX = mapHolder.scaleY = camera.getZoomFactor();			
			miniMap.redraw();
			camera.ScrollToCenter(center.x, center.y);
		}			

		public function onZoomIntoMinimap(e: Event):void {
			zoomIntoMinimap(!minimapZoomed);
		}

		public function zoomIntoMinimap(zoom: Boolean, query: Boolean = true) : void {
			if (minimapZoomed == false) {
				Global.map.camera.cue();
			}
			else {
				Global.map.camera.goToCue();
			}			
			
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
				miniMap.showLegend();
				miniMap.showPointers();
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
				miniMap.hideLegend();
				miniMap.hidePointers();
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
			pressedKeys[event.keyCode] = false;
		}		
		
		public function isKeyDown(keyCode: int) : Boolean
		{
			return pressedKeys[keyCode];
		}
		
		public function eventScroll(e: MouseEvent): void {		
			if (e.target is Component || e.target is TextField || frames.length > 0) return;
			
			if (e.delta < 0) {
				onZoomOut(e);
			}
			else if (e.delta > 0) {
				onZoomIn(e);				
			}
		}
		
		public function eventKeyDown(e: KeyboardEvent):void
		{
			// Key down handler
			
			// end key down handler
			
			// Key Press Handler
			if (pressedKeys[e.keyCode]) return;
			pressedKeys[e.keyCode] = true;
			
			// Escape key functions
			if (e.charCode == Keyboard.ESCAPE)
			{						
				// Unzoom map
				if (miniMap != null) zoomIntoMinimap(false);
				
				// Deselect objects
				clearAllSelections();
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
			stage.addEventListener(KeyboardEvent.KEY_DOWN, eventKeyDown);
			stage.addEventListener(MouseEvent.MOUSE_WHEEL, eventScroll);
			stage.addEventListener(KeyboardEvent.KEY_UP, eventKeyUp);
				
			this.map = map;
			this.miniMap = miniMap;

			// Clear current city list
			(lstCities.getModel() as VectorListModel).clear();

			// Add map			
			mapHolder.addChild(map);
			minimapHolder.addChild(miniMap);

			// Create map overlay
			this.mapOverlay = new MovieClip();
			this.mapOverlay.graphics.beginFill(0xCCFF00);
			this.mapOverlay.graphics.drawRect(0, 0, Constants.screenW, Constants.screenH);
			this.mapOverlay.visible = false;
			this.mapOverlay.mouseEnabled = false;
			this.mapOverlay.name = "Overlay";
			addChild(this.mapOverlay);

			// Populate city list
			for each (var city: City in map.cities) {
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

			// Create and position command line if admin
			cmdLine = new CmdLineViewer();
			cmdLine.show(cmdLineHolder);

			// Add objects to resize manager
			resizeManager = new ResizeManager(stage);

			resizeManager.addObject(this.mapOverlay, ResizeManager.ANCHOR_RIGHT | ResizeManager.ANCHOR_TOP | ResizeManager.ANCHOR_LEFT | ResizeManager.ANCHOR_BOTTOM);
			resizeManager.addObject(sidebarHolder, ResizeManager.ANCHOR_RIGHT | ResizeManager.ANCHOR_TOP);
			resizeManager.addObject(barBg, ResizeManager.ANCHOR_RIGHT | ResizeManager.ANCHOR_LEFT);
			resizeManager.addObject(resourcesContainer, ResizeManager.ANCHOR_TOP | ResizeManager.ANCHOR_RIGHT);
			resizeManager.addObject(miniMap, ResizeManager.ANCHOR_RIGHT | ResizeManager.ANCHOR_BOTTOM);
			resizeManager.addObject(minimapTools, ResizeManager.ANCHOR_RIGHT | ResizeManager.ANCHOR_BOTTOM);
			resizeManager.addObject(chains, ResizeManager.ANCHOR_RIGHT | ResizeManager.ANCHOR_TOP);
			if (cmdLine) resizeManager.addObject(cmdLine, ResizeManager.ANCHOR_BOTTOM | ResizeManager.ANCHOR_LEFT);

			resizeManager.addEventListener(Event.RESIZE, map.onResize);
			resizeManager.addEventListener(Event.RESIZE, message.onResize);

			resizeManager.forceMove();

			// Scroll to city center
			if (selectedCity) {
				var pt: Point = MapUtil.getScreenCoord(selectedCity.MainBuilding.x, selectedCity.MainBuilding.y);
				src.Global.gameContainer.camera.ScrollToCenter(pt.x, pt.y);
				miniMap.setCityPointer(selectedCity.name);
			}

			//Set minimap position and initial state
			miniMap.addEventListener(MiniMap.NAVIGATE_TO_POINT, onMinimapNavigateToPoint);			
			zoomIntoMinimap(false, false);
			
			// Refresh unread messages
			Global.mapComm.Messaging.refreshUnreadCounts();		

			// Begin game tutorial
			tutorial = new GameTutorial();
			tutorial.start(Constants.tutorialStep, map, Global.mapComm.General);
		}

		public function show() : void {
			// Create popup menu now that we have all the player info
			menu = new JPopupMenu();
			menu.addMenuItem("Profile").addActionListener(onProfileClick);
			menu.addMenuItem("Account Options").addActionListener(onAccountOptionsClick);
			menu.addMenuItem("Forums").addActionListener(onForumsClick);
			menu.addMenuItem("Wiki").addActionListener(onWikiClick);
			menu.addMenuItem("Help").addActionListener(onHelpClick);			
			menu.addMenuItem("Logout").addActionListener(onLogoutClick);

			// Reset camera pos
			camera.reset();

			// Close any previous open frames (Shouldnt really have any but just to be safe)
			closeAllFrames();

			// Set visible
			visible = true;

			// Create on screen message component, it'll auto show itself
			screenMessage = new ScreenMessagePanel(screenMessageHolder);
			
			// Add menu overlay
			addChild(menuDummyOverlay);
			
			// Start timers
			minimapRefreshTimer.start();
			resourcesTimer.start();
		
			// Show built in messages
			builtInMessages = new BuiltInMessages();
			builtInMessages.start();
		}

		public function dispose() : void {
			stage.removeEventListener(KeyboardEvent.KEY_DOWN, eventKeyDown);
			stage.removeEventListener(MouseEvent.MOUSE_WHEEL, eventScroll);
			stage.removeEventListener(KeyboardEvent.KEY_UP, eventKeyUp);			
			
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

			if (resizeManager) {
				resizeManager.removeAllObjects();
			}

			if (cmdLine) {
				if (cmdLine.getFrame()) {
					cmdLine.getFrame().dispose();
				}
				cmdLine = null;
			}
			
			if (tutorial) {
				tutorial.stop();
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
				minimapHolder.removeChild(miniMap);

				map = null;
				miniMap = null;
			}

			resizeManager = null;
		}
		
		public function addCityToUI(city: City): void {
			(lstCities.getModel() as VectorListModel).append( { id: city.id, city: city, toString: function() : String { return this.city.name; } } );
			miniMap.addPointer(new MiniMapPointer(city.MainBuilding.x, city.MainBuilding.y, city.name));
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
		
		public function getSidebar(): GameJSidebar
		{
			return this.sidebar;
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
			if (frame.isModal()) {						
				if (map != null) {
					clearAllSelections();
					map.disableMouse(true);				
				}
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
			if (frames.length == 0 && map != null) {
				map.disableMouse(false);
			}
			if (frames.length > 0) {
				frames[frames.length - 1].requestFocus();
			}
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
					miniMap.setCityPointer(selectedCity.name);
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
			
			stage.focus = map;
			miniMap.setCityPointer(selectedCity.name);
		}

		private function onMinimapNavigateToPoint(e: MouseEvent) : void {
			if (minimapZoomed) {
				zoomIntoMinimap(false);
			}

			Global.map.camera.ScrollToCenter(e.localX, e.localY);
		}
		
		public function setUnreadMessageCount(unreadMessages: int): void
		{
			txtUnreadMessages.visible = unreadMessages > 0;
			if (unreadMessages > 0) {
                txtUnreadMessages.txtUnreadCount.text = unreadMessages > 9 ? "!" : unreadMessages.toString();
                                
                Util.triggerJavascriptEvent("clientUnreadMessage");
            }
		}
		
		public function setUnreadBattleReportCount(unreadReports: int): void 
		{
			txtUnreadReports.visible = unreadReports > 0;				
			if (unreadReports > 0) {
                txtUnreadReports.txtUnreadCount.text = unreadReports > 9 ? "!" : unreadReports.toString();		
                
                Util.triggerJavascriptEvent("clientUnreadBattleReport");
            }
		}
		
		public function setUnreadForumIcon(flag: Boolean): void
		{
			tribeNotificationIcon.visible = flag;
            
            if (flag) {
                Util.triggerJavascriptEvent("clientUnreadForumMessage");
            }
		}
        
        public function setLabelCoords(pt: Point): void {
            minimapTools.txtCoords.text = "(" + (pt.x) + "," + (pt.y) + ")";
        }
	}

}


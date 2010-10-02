package src {
	import flash.display.*;
	import flash.events.*;
	import flash.geom.*;
	import flash.ui.Keyboard;
	import flash.utils.Timer;
	import org.aswing.event.AWEvent;
	import org.aswing.event.PopupEvent;
	import src.Components.MessageTimer;
	import src.Map.*;
	import src.Objects.Effects.*;
	import src.Objects.*;
	import src.Map.Map;
	import src.UI.Components.*;
	import src.UI.Components.ScreenMessages.ScreenMessagePanel;
	import src.UI.Dialog.*;
	import src.UI.*;
	import flash.ui.*;

	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;

	public class GameContainer extends GameContainer_base {

		//City list
		private var lstCities: JComboBox;

		//Currently selected sidebar
		private var sidebar: GameJSidebar;

		//Container for sidebar
		private var sidebarHolder: Sprite;

		public var map: Map;
		public var miniMap: MiniMap;

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
		private var minimapZoomed: Boolean = false;
		private var minimapZoomTooltip: SimpleTooltip;

		//Handles fancy auto resizing
		private var resizeManager: ResizeManager;

		// Game bar bg. Can't be in Flash because of scale9 issue
		private var barBg: DisplayObject;

		// Username + logout link
		private var userPnl: JPanel;

		// Command line
		private var cmdLine: CmdLineViewer;

		public function GameContainer()
		{
			// Create and position the city list
			lstCities = new JComboBox();
			lstCities.setModel(new VectorListModel());
			lstCities.addActionListener(onChangeCitySelection);
			lstCities.setSize(new IntDimension(128, 22));
			lstCities.setLocation(new IntPoint(37, 12));
			addChild(lstCities);

			// Create and position the username panel
			userPnl = new JPanel(new FlowLayout(AsWingConstants.RIGHT));
			userPnl.setPreferredSize(new IntDimension(200, 22));
			userPnl.setLocation(new IntPoint(480, 12));
			addChild(userPnl);

			// Create barBg
			var barBgClass: Class = UIManager.getDefaults().get("GameMenu.bar");
			barBg = new barBgClass() as DisplayObject;
			addChildAt(barBg, 1);

			// Hide the new count bubble for messaged and reports
			txtUnreadMessages.visible = false;
			txtUnreadMessages.mouseChildren = false;
			txtUnreadMessages.mouseEnabled = false;

			txtUnreadReports.visible = false;
			txtUnreadReports.mouseChildren = false;
			txtUnreadReports.mouseEnabled = false;

			// Add key down listener to stage
			addEventListener(Event.ADDED_TO_STAGE, function(e: Event):void {
				stage.addEventListener(KeyboardEvent.KEY_DOWN, eventKeyDown);
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

			new SimpleTooltip(minimapTools.btnGoToCoords, "Go to...");
			minimapTools.btnGoToCoords.addEventListener(MouseEvent.CLICK, onGoToCoords);

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

			// Set up sidebar holder
			sidebarHolder = new Sprite();
			sidebarHolder.x = Constants.screenW - GameJSidebar.WIDTH - 15;
			sidebarHolder.y = 60;
			addChildAt(sidebarHolder, 1);

			//Set up resources timer
			resourcesTimer.addEventListener(TimerEvent.TIMER, displayResources);
			resourcesTimer.start();
		}

		public function onViewCityTroops(e: MouseEvent) :void
		{
			if (!selectedCity)
			return;

			var movementDialog: MovementDialog = new MovementDialog(selectedCity);
			movementDialog.show();
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
				map.scrollRate = 4;
			}
			else {
				screenMessage.setVisible(true);
				miniMap.resize(Constants.miniMapScreenW, Constants.miniMapScreenH);
				miniMap.x = Constants.miniMapScreenX(Constants.miniMapScreenW);
				miniMap.y = Constants.miniMapScreenY(Constants.miniMapScreenH);
				minimapZoomTooltip.setText("World view");
				miniMap.setScreenRectHidden(false);
				map.disableMapQueries(false);
				map.scrollRate = 1;
			}

			minimapZoomed = zoom;
			if (query) {
				map.onMove();
			}

			alignMinimapTools();
		}

		public function eventKeyDown(e: KeyboardEvent):void
		{
			if (e.charCode == Keyboard.ESCAPE)
			{
				if (map != null) {
					map.selectObject(null);
				}

				if (miniMap != null) {
					zoomIntoMinimap(false);
				}
			}
		}

		public function setMap(map: Map, miniMap: MiniMap):void
		{
			this.map = map;
			this.miniMap = miniMap;

			// Populate user panel
			/*
			userPnl.removeAll();
			var userLblMaker: Function = function(txt: String) : JLabel {
			var lbl: JLabel = new JLabel(txt);
			GameLookAndFeel.changeClass(lbl, "Tooltip.text Label.small");
			lbl.pack();
			return lbl;
			}
			userPnl.append(userLblMaker(map.usernames.players.get(Constants.playerId).name + "("));
			userPnl.append(userLblMaker(")"));
			userPnl.pack();
			*/

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
				(lstCities.getModel() as VectorListModel).append( { id: city.id, city: city, toString: function() : String { return city.name; } } );
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
				cmdLine.getFrame().setLocationXY(300, Constants.screenH - 292);
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
			resizeManager.addObject(userPnl, ResizeManager.ANCHOR_RIGHT | ResizeManager.ANCHOR_TOP);
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
			// Reset camera pos
			camera.reset();

			// Close any previous open frames (Shouldnt really have any but just to be safe)
			closeAllFrames();

			// Set visible
			visible = true;

			// Create on screen message component, it'll auto show itself
			screenMessage = new ScreenMessagePanel();

			// Create message timer to check for new msgs
			messageTimer = new MessageTimer();
			messageTimer.start();
		}

		public function dispose() : void {
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
			setSidebar(null);

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

		private function alignMinimapTools() : void {
			minimapTools.x = miniMap.x;
			minimapTools.y = miniMap.y - 3;
		}

		public function setSidebar(sidebar: GameJSidebar):void
		{
			if (this.sidebar != null) {
				this.sidebar.getFrame().dispose();
			}

			chains.visible = false;
			this.sidebar = sidebar;

			if (sidebar != null) {
				chains.visible = true;
				sidebar.show(sidebarHolder);
			}

			stage.focus = null;
		}

		public function closeAllFrames() : void {
			var framesCopy: Array = frames.concat();

			for (var i: int = framesCopy.length - 1; i >= 0; --i)
			(framesCopy[i] as JFrame).dispose();
		}

		public function showFrame(frame: JFrame):void {
			if (map != null) map.disableMouse(true);
			frames.push(frame);
			frame.addEventListener(PopupEvent.POPUP_CLOSED, onFrameClosing);
			frame.show();
		}

		public function onFrameClosing(e: PopupEvent):void {
			var frame: JFrame = e.target as JFrame;
			if (frame == null) return;
			frame.removeEventListener(PopupEvent.POPUP_CLOSED, onFrameClosing);
			var index: int = frames.indexOf(frame);
			if (index == -1) trace("Closed a frame that did not call show through GameContainer");
			frames.splice(index, 1);
			if (frames.length == 0 && map != null) map.disableMouse(false);
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

		public function onChangeCitySelection(e: AWEvent):void {
			selectedCity = null;
			if (lstCities.getSelectedIndex() == -1) return;

			selectedCity = lstCities.getSelectedItem().city;

			displayResources();
		}

		private function onMinimapNavigateToPoint(e: MouseEvent) : void {
			if (minimapZoomed) {
				zoomIntoMinimap(false);
			}

			Global.map.camera.ScrollToCenter(e.localX, e.localY);
		}
	}

}


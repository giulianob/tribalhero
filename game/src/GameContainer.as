package src {
	import flash.display.*;
	import flash.events.*;
	import flash.geom.*;
	import flash.ui.Keyboard;
	import flash.utils.Timer;
	import org.aswing.event.AWEvent;
	import org.aswing.event.PopupEvent;
	import src.Map.*;
	import src.Objects.Effects.*;
	import src.Objects.*;
	import src.Map.Map;
	import src.UI.Components.*;
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

		//Minimap background image
		private var miniMapMask: Sprite;

		//Holds all currently open aswing frames
		private var frames: Array = new Array();

		public var selectedCity: City;
		public var camera: Camera = new Camera(0, 0);

		//Resources timer that fires every second
		public var resourcesTimer: Timer = new Timer(1000);

		public function GameContainer()
		{
			lstCities = new JComboBox();
			lstCities.setModel(new VectorListModel());
			lstCities.addActionListener(onChangeCitySelection);
			lstCities.setSize(new IntDimension(128, 22));
			lstCities.setLocation(new IntPoint(25, 10));
			addChild(lstCities);

			addEventListener(Event.ADDED_TO_STAGE, function(e: Event):void {
				stage.addEventListener(KeyboardEvent.KEY_DOWN, eventKeyDown);
			}
			);

			visible = false;

			txtCoords.mouseEnabled = false;

			new SimpleTooltip(btnGoToCoords, "Go to...");
			btnGoToCoords.addEventListener(MouseEvent.CLICK, onGoToCoords);

			new SimpleTooltip(btnGoToCity, "Go to city");
			btnGoToCity.addEventListener(MouseEvent.CLICK, onGoToCity);

			new SimpleTooltip(btnMessages, "View battle reports");
			btnMessages.addEventListener(MouseEvent.CLICK, onViewMessages);

			new SimpleTooltip(btnCityInfo, "View city details");
			btnCityInfo.addEventListener(MouseEvent.CLICK, onViewCityInfo);

			new SimpleTooltip(btnCityTroops, "View unit movement");
			btnCityTroops.addEventListener(MouseEvent.CLICK, onViewCityTroops);

			miniMapHolder.mouseEnabled = false;
			miniMapHolder.mouseChildren = false;

			miniMapMask = new Sprite();
			miniMapMask.graphics.beginFill(0x336699);
			miniMapMask.graphics.drawRect(miniMapHolder.x, miniMapHolder.y, miniMapHolder.width, miniMapHolder.height);
			miniMapMask.graphics.endFill();

			sidebarHolder = new Sprite();
			addChild(sidebarHolder);

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

		public function onViewMessages(e: MouseEvent):void
		{
			var battleReportDialog: BattleReportList = new BattleReportList();
			battleReportDialog.show();
		}

		public function onGoToCity(e: Event) : void {
			if (selectedCity == null) return;

			var pt: Point = MapUtil.getScreenCoord(selectedCity.MainBuilding.x, selectedCity.MainBuilding.y);
			Global.gameContainer.map.camera.ScrollToCenter(pt.x, pt.y);
		}

		public function onGoToCoords(e: Event):void {
			var goToDialog: GoToDialog = new GoToDialog();
			goToDialog.show();
		}

		public function eventKeyDown(e: KeyboardEvent):void
		{
			if (e.charCode == Keyboard.ESCAPE)
			{
				if (map != null)
				map.selectObject(null);
			}
		}

		public function setMap(map: Map, miniMap: MiniMap):void
		{
			this.map = map;
			Global.map = map;
			this.miniMap = miniMap;
			RequirementFormula.map = map;
			(lstCities.getModel() as VectorListModel).clear();

			if (map != null)
			{
				mapHolder.addChild(map);
				map.setGameContainer(this);

				this.mapOverlay = new MovieClip();
				this.mapOverlay.graphics.beginFill(0xCCFF00);
				this.mapOverlay.graphics.drawRect(0, 0, mapHolder.width, mapHolder.height);
				this.mapOverlay.visible = false;
				this.mapOverlay.mouseEnabled = false;
				this.mapOverlay.name = "Overlay";
				this.mapOverlay.x = mapHolder.x;
				this.mapOverlay.y = mapHolder.y;
				addChild(this.mapOverlay);

				for each (var city: City in map.cities.each()) {
					(lstCities.getModel() as VectorListModel).append( { id: city.id, city: city, toString: function() : String { return city.name; } } );
				}

				if (lstCities.getItemCount() > 0) { //set a default city selection
					lstCities.setSelectedIndex(0);
					selectedCity = lstCities.getSelectedItem().city;
					var pt: Point = MapUtil.getScreenCoord(selectedCity.MainBuilding.x, selectedCity.MainBuilding.y);
					map.gameContainer.camera.ScrollToCenter(pt.x, pt.y);
				}

				resourcesContainer = new ResourcesContainer();
				displayResources();
			}

			if (miniMap != null)
			{
				miniMapHolder.addChild(miniMap);
			}
		}

		public function show() : void {
			camera.reset();
			visible = true;
		}

		public function dispose() : void {
			visible = false;

			if (resourcesContainer && resourcesContainer.getFrame())
			resourcesContainer.getFrame().dispose();

			resourcesContainer = null;
			setSidebar(null);

			if (map != null)
			{

				map.dispose();
				mapHolder.removeChild(map);
				removeChild(mapOverlay);
			}

			if (miniMap != null)
			miniMapHolder.removeChild(miniMap);

			map = null;
			miniMap = null;
		}

		public function setSidebar(sidebar: GameJSidebar):void
		{
			if (this.sidebar != null)
			this.sidebar.getFrame().dispose();

			this.sidebar = sidebar;

			if (sidebar != null)
			sidebar.show(sidebarHolder);
		}

		public function closeAllFrames() : void {
			var framesCopy: Array = frames.concat();

			for (var i: int = framesCopy.length - 1; i >= 0; --i)
			(framesCopy[i] as JFrame).dispose();
		}

		public function showFrame(frame: JFrame):void {
			if (map != null) map.disableMouse();
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
			if (frames.length == 0 && map != null) map.enableMouse();
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
	}

}


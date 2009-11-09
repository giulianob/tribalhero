package src {	
	import fl.controls.Button;
	import flash.display.*;
	import flash.events.*;	
	import flash.external.ExternalInterface;
	import flash.geom.*;
	import flash.text.TextFieldAutoSize;
	import flash.ui.Keyboard;
	import flash.utils.Timer;
	import org.aswing.event.PopupEvent;
	import src.Map.*;
	import src.Objects.Effects.*;
	import src.Objects.*;
	import src.Map.Map;
	import src.UI.Components.*;
	import src.UI.Dialog.*;
	import src.UI.*;
	import fl.data.DataProvider;
	import flash.ui.*;
	import src.Util.Util;
	
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;	
	
	public class GameContainer extends GameContainer_base {
		
		private var sidebar: GameJSidebar;
		private var sidebarHolder: Sprite;
		public var map: Map;
		public var miniMap: MiniMap;
		private var mapOverlay: Sprite;
		private var mapOverlayTarget: Sprite;
		private var resourcesContainer: GameJBox;
		
		private var miniMapMask: Sprite;
		private var dialogHolder: Sprite;
		
		private var overlay: Sprite;
		private var dialogs: Array = new Array();
		private var frames: Array = new Array();
		
		private var menuItems: PaintBox;
		
		public var selectedCity: City;
		
		public var camera: Camera = new Camera(0, 0);
		
		public var resourcesTimer: Timer = new Timer(1000);
		
		public function GameContainer()
		{		
			addEventListener(Event.ADDED_TO_STAGE, function(e: Event):void {
					stage.addEventListener(KeyboardEvent.KEY_DOWN, eventKeyDown);
					
					if (!stage.contains(dialogHolder))
						stage.addChild(dialogHolder);
				}
			);
			
			dialogHolder = new Sprite();
			dialogHolder.name = "DialogHolder";
			dialogHolder.x = 0;
			dialogHolder.y = 0;
			
			visible = false;
			
			txtCoords.mouseEnabled = false;
			
			new SimpleTooltip(btnGoToCoords, "Go to...");
			btnGoToCoords.addEventListener(MouseEvent.CLICK, onGoToCoords);
			
			new SimpleTooltip(btnMessages, "View messages");
			btnMessages.addEventListener(MouseEvent.CLICK, onViewMessages);
			
			new SimpleTooltip(btnCityInfo, "View city details");
			btnCityInfo.addEventListener(MouseEvent.CLICK, onViewCityInfo);
			
			new SimpleTooltip(btnCityTroops, "View city troops");
			btnCityTroops.addEventListener(MouseEvent.CLICK, onViewCityTroops);			
			
			lstCities.addEventListener(Event.CHANGE, onChangeCitySelection);
			
			overlay = getScreenOverlay();						
			
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
		
		public function getScreenOverlay(): Sprite
		{
			var overlay:Sprite = new Sprite();
			overlay.mouseEnabled = true;
			overlay.graphics.beginFill(0x0A0A0A, 0.30);
            overlay.graphics.lineStyle(0);
			overlay.graphics.drawRect(0, 0, Constants.movieW, Constants.movieH);
			overlay.graphics.endFill();										
			
			return overlay;
		}

		public function onViewCityTroops(e: MouseEvent) :void
		{
			if (!selectedCity)
				return;
			
			var cityInfoDialog: CityInfoDialog = new CityInfoDialog();
			cityInfoDialog.init(map, selectedCity, function(sender: Dialog):void { closeDialog(sender); } );
			
			showDialog(cityInfoDialog);					
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
			try
			{
				ExternalInterface.call("showMessages");
			}
			catch (error: Error) {}
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
					map.doSelectedObject(null);
			}
		}
		
		public function setMap(map: Map, miniMap: MiniMap):void
		{						
			this.map = map;
			Global.map = map;
			this.miniMap = miniMap;
			RequirementFormula.map = map;			
			lstCities.dataProvider = new DataProvider();
			
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
					lstCities.dataProvider.addItem( { label: city.name, id: city.id, city: city } );
				}
				
				if (lstCities.dataProvider.length > 0) { //set a default city selection
					selectedCity = lstCities.dataProvider.getItemAt(0).city;
					lstCities.selectedIndex = 0;		
					var pt: Point = MapUtil.getScreenCoord(selectedCity.MainBuilding.x, selectedCity.MainBuilding.y);
					map.gameContainer.camera.ScrollToCenter(pt.x, pt.y);					
				}												
				
				resourcesContainer = new GameJBox();
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
			
			closeAllDialogs();
			
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
		
		public function closeAllDialogs() :void
		{
			var size: int = dialogs.length;
			for (var i: int = 0; i < size; i++)
				closeDialog();
		}
		
		public function closeDialog(dialog: Dialog = null):void
		{
			var idx: int = -1;			
			
			if (dialog)
			{				
				for (var i: int = 0; i < dialogs.length; i++)
				{
					if (dialogs[i] == dialog)
					{
						idx = i;
						break;
					}
				}				
			}	
			else
			{					
				idx = dialogs.length - 1;
				dialog = dialogs[idx];
			}
			
			if (idx == -1)
				return;			
						
			var disposeTmp: IDisposable = dialog as IDisposable;
			
			dialogHolder.removeChild(dialog);
			dialogHolder.removeChild(overlay);
			
			dialogs.splice(idx, 1);			
			
			if (disposeTmp != null)
				disposeTmp.dispose();
				
			if (dialogs.length == 0)
			{
				if (map != null)
					map.enableMouse();					
			} 
			else
			{
				dialogs[dialogs.length - 1].enableInput();
				dialogHolder.addChildAt(overlay, dialogHolder.getChildIndex(dialogs[dialogs.length - 1]));				
				overlay.visible = (dialogs[dialogs.length - 1] as Dialog).hasFadedBackground();				
			}
			
			repositionDialogs();		
			
			stage.focus = null;
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
		
		public function showDialog(dialog: Dialog):void
		{									
			if (dialogs.length > 0)
				dialogs[dialogs.length - 1].disableInput();									
			
			dialogs.push(dialog);
			
			if (dialogHolder.contains(overlay))			
				dialogHolder.removeChild(overlay);			
			
			dialogHolder.addChild(overlay);
			overlay.visible = (dialogs[dialogs.length - 1] as Dialog).hasFadedBackground();
			dialogHolder.addChild(dialog);
			
			if (map != null)
				map.disableMouse();
				
			repositionDialogs();
			
			stage.focus = null;
		}
		
		private function repositionDialogs():void
		{
			for (var i: int = 0; i < dialogs.length; i++)
			{
				var depth: int = dialogs.length - 1 - i;
			
				var dialog: Dialog = dialogs[i];
				
				depth = 0;//disabled cascade
				
				dialog.x = int(Constants.movieW / 2 - dialog.getSmartWidth() / 2) - depth * 20;
				dialog.y = int(Constants.movieH / 2 - dialog.getSmartHeight() / 2) - depth * 20 + 25;
			}
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
		
		public function displayResources(e: Event = null):void
		{										
			if (!resourcesContainer) return;
			
			if (selectedCity == null)
			{
				if (resourcesContainer.getFrame())
					resourcesContainer.getFrame().dispose();				
				
				return;									
			}
			
			selectedCity.dispatchEvent(new Event(City.RESOURCES_UPDATE));
			
			var resourceLabelMaker: Function = function(value: int, max : int, icon: Icon = null) : JLabel {				
				var label: JLabel = new JLabel(value.toString(), icon);
				if (max != -1 && value >= max)
					GameLookAndFeel.changeClass(label, "Label.success Label.small");
				else
					GameLookAndFeel.changeClass(label, "Tooltip.text Label.small");
					
				label.setIconTextGap(0);			
				label.setHorizontalTextPosition(AsWingConstants.LEFT);				
				return label;
			};						
			
			resourcesContainer.setLayout(new FlowLayout(AsWingConstants.LEFT, 10, 5, true));
			resourcesContainer.removeAll();
			
			resourcesContainer.append(resourceLabelMaker(selectedCity.resources.labor.getValue(), -1, new AssetIcon(new ICON_LABOR())));
			resourcesContainer.append(resourceLabelMaker(selectedCity.resources.gold.getValue(), -1, new AssetIcon(new ICON_GOLD())));			
			resourcesContainer.append(resourceLabelMaker(selectedCity.resources.wood.getValue(), selectedCity.resources.wood.getLimit(), new AssetIcon(new ICON_WOOD())));			
			resourcesContainer.append(resourceLabelMaker(selectedCity.resources.crop.getValue(), selectedCity.resources.crop.getLimit(), new AssetIcon(new ICON_CROP())));			
			resourcesContainer.append(resourceLabelMaker(selectedCity.resources.iron.getValue(), selectedCity.resources.iron.getLimit(), new AssetIcon(new ICON_IRON())));			
			
			if (!resourcesContainer.getFrame())
				resourcesContainer.show()
			
			resourcesContainer.getFrame().pack();			
			resourcesContainer.getFrame().setLocationXY(Constants.screenW - resourcesContainer.getFrame().getWidth() + 10, 3);
		}
		
		public function onChangeCitySelection(e: Event):void {		
			selectedCity = null;
			if (lstCities.selectedIndex == -1) return;
			
			selectedCity = lstCities.selectedItem.city;
			
			displayResources();
		}
	}
	
}
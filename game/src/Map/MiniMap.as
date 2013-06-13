package src.Map
{
	import flash.display.Sprite;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import flash.geom.Point;
	import mx.messaging.ConsumerMessageDispatcher;
	import org.aswing.ASColor;
    import org.aswing.AsWingManager;
    import org.aswing.AsWingUtils;
	import org.aswing.graphics.Graphics2D;
	import org.aswing.graphics.SolidBrush;
	import src.Constants;
	import src.Map.CityRegionFilters.*;
	import src.UI.Components.MiniMapPointer;
	import src.Util.Util;
	import src.Global;
	import src.Objects.ObjectContainer;
	import System.Linq.Enumerable;

	/**
	 * ...
	 * @author Giuliano Barberi
	 *
	 */
	public class MiniMap extends Sprite
	{
		public static const NAVIGATE_TO_POINT: String = "NAVIGATE_TO_POINT";

		private var regionSpace: Sprite;
		private var regions: CityRegionList;
		private var filter: CityRegionFilter = new CityRegionFilter();
		private var legend: CityRegionLegend = new CityRegionLegend();
		private var pendingRegions: Array = [];

		public var objContainer: ObjectContainer;

		private var screenRect: Sprite;
		private var mapHolder: Sprite;
		private var bg: Sprite;
		private var mapMask: Sprite;
		
		private var pointers: Array = [];
		private var pointersVisible: Boolean = false;
		private var cityPointer: MiniMapPointer;
		
		private var miniMapWidth: int;
		private var miniMapHeight: int;

		private var lastClick: Number;
		private var lastClickPoint: Point = new Point();

		public function MiniMap(width: int, height: int)
		{
			regions = new CityRegionList();

			regionSpace = new Sprite();

			objContainer = new ObjectContainer(false, false);

			mapHolder = new Sprite();

			screenRect = new Sprite();
			
			mapHolder.addChild(regionSpace);
			mapHolder.addChild(objContainer);
			mapHolder.addChild(screenRect);


			bg = new Sprite();

			addEventListener(MouseEvent.CLICK, onNavigate);

			mapMask = new Sprite();

			addChild(bg);
			addChild(mapHolder);
			addChild(mapMask);

			mask = mapMask;

			resize(width, height);
			
			legend.addOnClickListener(onChangeFilter);
			
			addEventListener(Event.REMOVED_FROM_STAGE, function(e: Event): void {
				legend.hide();
			});
		}

		public function redraw() : void {
			// Redraw screen rectangle
			var tilesW: Number = (Constants.screenW * Global.gameContainer.camera.getZoomFactorOverOne()) / Constants.tileW + 0.5;
			var tilesH: Number = (Constants.screenH * Global.gameContainer.camera.getZoomFactorOverOne()) / Constants.tileH + 0.5;

			if (tilesW * Constants.miniMapTileW < this.miniMapWidth && tilesH * Constants.miniMapTileH < this.miniMapHeight) {			
				screenRect.graphics.clear();
				screenRect.graphics.lineStyle(1, 0xFFFFFF, 0.8);
				screenRect.graphics.drawRect(0, 0, tilesW * Constants.miniMapTileW, tilesH * Constants.miniMapTileH);
			}

			// Resize map
			mapHolder.x = (this.miniMapWidth / 2) - (screenRect.width / 2);
			mapHolder.y = (this.miniMapHeight / 2) - (screenRect.height / 2);

			bg.graphics.clear();
			var g: Graphics2D = new Graphics2D(bg.graphics);
			bg.alpha = 0.8;
			g.fillRoundRect(new SolidBrush(ASColor.BLACK), 0, 0, this.miniMapWidth, this.miniMapHeight, 10);

			mapMask.graphics.clear();
			g = new Graphics2D(mapMask.graphics);
			g.fillRoundRect(new SolidBrush(ASColor.BLACK), 0, 0, this.miniMapWidth, this.miniMapHeight, 10);	
            
            alignLegend();
		}
		
		public function setFilter(name:String) : Boolean {
			if (name == "Default") {
				filter = new CityRegionFilter();
			} else if (name == "Alignment") {
				filter = new CityRegionFilterAlignment();
			} else if (name == "Distance") {
				filter = new CityRegionFilterDistance();
			} else if (name == "Tribe") {
				filter = new CityRegionFilterTribe();
			} else if (name == "Newbie") {
				filter = new CityRegionFilterNewbie();
			} else {
				return false;
			}
			for each(var region:CityRegion in regions) {
				region.setFilter(filter);
			}
			showLegend();
			return true;
		}
		
		public function onChangeFilter(e:Event): void {
			if (filter.getName() == "Default") {
				setFilter("Alignment");
			} else if (filter.getName() == "Alignment") {
				setFilter("Distance");
			} else if (filter.getName() == "Distance") {
				setFilter("Tribe");
			} else if (filter.getName() == "Tribe") {
				setFilter("Newbie");
			} else if (filter.getName() == "Newbie") {
				setFilter("Default");
			}
			redraw();
		}
		
		public function showLegend() : void {
			legend.removeAll();
			filter.applyLegend(legend);
			legend.show(this.x + this.miniMapWidth, this.y);
		}
        
        public function alignLegend() : void {
            legend.align(this.x + this.miniMapWidth, this.y);
        }
		
		public function hideLegend() : void {
			legend.hide();
		}

		private function onNavigate(e: MouseEvent) : void {

			var currentMousePoint: Point = new Point(stage.mouseX, stage.mouseY);
			if (Point.distance(currentMousePoint, lastClickPoint) > 10 || (new Date().time) - lastClick > 350) {
				lastClick = new Date().time;
				lastClickPoint = currentMousePoint;
				return;
			}

			lastClick = new Date().time;
			//Calculate where the user clicked in real map position
			var camX: int = Global.gameContainer.camera.miniMapX - mapHolder.x;
			var camY: int = Global.gameContainer.camera.miniMapY - mapHolder.y;

			var local: Point = this.globalToLocal(new Point(e.stageX, e.stageY));
			var centeredOffsetX: int = local.x;
			var centeredOffsetY: int = local.y;

			var mapX: int = ((camX + centeredOffsetX) / Constants.miniMapTileW) * Constants.tileW;
			var mapY: int = ((camY + centeredOffsetY) / Constants.miniMapTileH) * Constants.tileH;

			var event: MouseEvent = new MouseEvent(NAVIGATE_TO_POINT, true, false, mapX, mapY);
			dispatchEvent(event);
		}

		public function resize(width: int, height: int) : void {
			this.miniMapWidth = width;
			this.miniMapHeight = height;
            redraw();            
		}

		public function setScreenRectHidden(hidden: Boolean) : void {
			screenRect.visible = !hidden;
		}

		public function addCityRegion(id:int) : CityRegion
		{
			if (Constants.debug >= 2)
			Util.log("Adding region: " + id);

			var newRegion: CityRegion = new CityRegion(id,filter);

			for (var i:int = pendingRegions.length - 1; i >= 0; i--)
			{
				if (pendingRegions[i] == id)
				{
					pendingRegions.splice(i, 1);
				}
			}

			regions.add(newRegion);
			newRegion.moveWithCamera(Global.gameContainer.camera);
			regionSpace.addChild(newRegion);

			return newRegion;
		}

		public function parseRegions(forceParse: Boolean = false):void
		{
			if (Constants.debug >= 4) {
				Util.log("on move: " + Global.gameContainer.camera.miniMapX + "," + Global.gameContainer.camera.miniMapY);
			}
			
			// Remove all regions if we are forcing
			if (forceParse) {
				for each (var region: CityRegion in regions) {
					region.disposeData();
					regionSpace.removeChild(region);					
				}
				
				regions.clear();
			}

			//calculate which regions we need to render
			var requiredRegions: Array = [];

			var camX: int = Global.gameContainer.camera.miniMapX - mapHolder.x;
			var camY: int = Global.gameContainer.camera.miniMapY - mapHolder.y;

			var regionsW: int = Math.ceil(miniMapWidth / Constants.cityRegionW);
			var regionsH: int = Math.ceil(miniMapHeight / (Constants.cityRegionH / 2));

			for (var c: int = 0; c <= regionsW; c++) {
				for (var r: int = 0; r <= regionsH; r++) {
					var requiredId: int = MapUtil.getCityRegionId(camX + Constants.cityRegionW * c, camY + (Constants.cityRegionH / 2) * r);
					if (requiredId == -1 || requiredRegions.indexOf(requiredId) > -1) continue;
					requiredRegions.push(requiredId);
				}
			}

			//remove any outdated regions from regions we have
			for (var i: int = regions.size() - 1; i >= 0; i--)
			{
				region = regions.getByIndex(i);

				var found: int = -1;
				for (var a:int= 0; a < requiredRegions.length; a++)
				{
					if (region.id == requiredRegions[a])
					{
						found = a;
						break;
					}
				}

				if (found >= 0)
				{
					//adjust the position of this region
					region.moveWithCamera(Global.gameContainer.camera);

					if (Constants.debug >= 4)
					Util.log("Moved: " + region.id + " " + region.x + "," + region.y);

					//remove it from required regions since we already have it
					requiredRegions.splice(found, 1);
				}
				else
				{
					//region is outdated, remove it from buffer
					if (Constants.debug >= 4)
					Util.log("Discarded: " + region.id);
					region.disposeData();
					regionSpace.removeChild(region);
					regions.removeByIndex(i);
				}
			}

			//remove any pending regions from the required regions list we need
			//and add any regions we are going to be asking the server to the pending regions list
			for (i = requiredRegions.length - 1; i >= 0; i--)
			{
				found = -1;

				for (a = 0; a < pendingRegions.length; a++)
				{
					if (pendingRegions[a] == requiredRegions[i])
					{
						found = i;
						break;
					}
				}

				if (found >= 0)
				{
					requiredRegions.splice(found, 1);
				}
				else
				{
					pendingRegions.push(requiredRegions[i]);
				}
			}

			//regions that we still need, query server
			if (requiredRegions.length > 0)
			{
				if (Constants.debug >= 3)
				Util.log("Required:" + requiredRegions);

				Global.mapComm.Region.getCityRegion(requiredRegions);
			}
		}
		
		public function addPointer(pointer: MiniMapPointer): void {
			pointer.visible = pointersVisible;
			pointers.push(pointer);
			addChild(pointer);
		}
		
		public function setCityPointer(name: String): void {
			if (cityPointer != null && cityPointer.getPointerName() != name) {
				cityPointer.setIcon(new ICON_MINIMAP_ARROW_BLUE());
			} else if (cityPointer!=null) {
				return;
			}
			
			cityPointer = Enumerable.from(pointers).first(function(p:MiniMapPointer):Boolean {
				return p.getPointerName() == name;
			});
			if (cityPointer != null) {
				cityPointer.setIcon(new ICON_MINIMAP_ARROW_RED());
			}
		}
		
		public function showPointers(): void {
			if (pointersVisible) return;
			pointersVisible = true;
			for each(var pointer:MiniMapPointer in pointers) {
				pointer.visible = true;
			}
		}
		
		public function hidePointers(): void {
			if (!pointersVisible) return;
			pointersVisible = false;
			for each(var pointer:MiniMapPointer in pointers) {
				pointer.visible = false
			}
		}
		
		public function updatePointers(center: Point): void {
			if (!pointersVisible) return;
			for each(var pointer:MiniMapPointer in pointers) {
				pointer.update(center, miniMapWidth, miniMapHeight);
			}
		}
	}

}


package src.Map.MiniMap
{
    import System.Linq.Enumerable;

    import flash.geom.Point;

    import src.Constants;
    import src.Events.NavigateEvent;
    import src.Global;
    import src.Map.*;
    import src.Objects.Factories.SpriteFactory;
    import src.Objects.ObjectContainer;
    import src.UI.Components.MiniMapPointer;
    import src.Util.Util;

    import starling.display.*;
    import starling.display.graphics.RoundedRectangle;
    import starling.events.*;
    import starling.extensions.pixelmask.PixelMaskDisplayObject;

    public class MiniMap extends Sprite
	{
		public static const NAVIGATE_TO_POINT: String = "NAVIGATE_TO_POINT";

        private var maskedContainer:PixelMaskDisplayObject;
		private var regionSpace: Sprite;
		private var regions: MiniMapRegionList;
		private var mapFilter: MiniMapDrawer = new MiniMapDrawer();
		private var legend: MiniMapLegend = new MiniMapLegend();
		private var pendingRegions: Array = [];

		public var objContainer: ObjectContainer;

		private var screenRect: Shape;
		private var mapHolder: Sprite;
		private var bg: Sprite;

		private var pointers: Array = [];
		private var pointersVisible: Boolean = false;
		private var cityPointer: MiniMapPointer;
		
		private var miniMapWidth: int;
		private var miniMapHeight: int;

		private var lastClick: Number;
		private var lastClickPoint: Point = new Point();

		public function MiniMap(width: int, height: int)
		{
            addEventListener(TouchEvent.TOUCH, onNavigate);

			regions = new MiniMapRegionList();

			regionSpace = new Sprite();

			objContainer = new ObjectContainer(false, false);

			mapHolder = new Sprite();

			screenRect = new Shape();
			
			mapHolder.addChild(regionSpace);
			mapHolder.addChild(objContainer);
			mapHolder.addChild(screenRect);
			bg = new Sprite();

            maskedContainer = new PixelMaskDisplayObject(-1, false);
            maskedContainer.addChild(bg);
            maskedContainer.addChild(mapHolder);

            addChild(maskedContainer);

			resize(width, height);
			
            mapFilter.addOnChangeListener(onFilterChange);
            mapFilter.applyLegend(legend);

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

            bg.removeChildren();
            var bgRect:RoundedRectangle = new RoundedRectangle(this.miniMapWidth, this.miniMapHeight);
            bgRect.material.color = 0x000000;
            bgRect.alpha = 0.8;
            bg.addChild(bgRect);

            maskedContainer.mask = new RoundedRectangle(this.miniMapWidth, this.miniMapHeight);

            alignLegend();
		}
		
        public function onFilterChange():void {
            for each(var region:MiniMapRegion in regions) {
                region.setFilter(mapFilter);
            }
            showLegend();
            redraw();
        }
		
		public function showLegend() : void {
			legend.show(this.x + this.miniMapWidth, this.y);
		}
        
        public function alignLegend() : void {
            legend.align(this.x + this.miniMapWidth, this.y);
        }
		
		public function hideLegend() : void {
			legend.hide();
		}

		private function onNavigate(e: TouchEvent) : void {

            var touch: Touch = e.getTouch(this, TouchPhase.ENDED);

            if (touch != null) {

                var currentMousePoint: Point = new Point(touch.globalX, touch.globalY);
                if (Point.distance(currentMousePoint, lastClickPoint) > 10 || (new Date().time) - lastClick > 350) {
                    lastClick = new Date().time;
                    lastClickPoint = currentMousePoint;
                    return;
                }

                lastClick = new Date().time;
                //Calculate where the user clicked in real map position
                var camX: int = Global.gameContainer.camera.miniMapX - mapHolder.x;
                var camY: int = Global.gameContainer.camera.miniMapY - mapHolder.y;

                var local: Point = touch.getLocation(this);
                var centeredOffsetX: int = local.x;
                var centeredOffsetY: int = local.y;

                var mapX: int = ((camX + centeredOffsetX) / Constants.miniMapTileW) * Constants.tileW;
                var mapY: int = ((camY + centeredOffsetY) / Constants.miniMapTileH) * Constants.tileH;

                var event: NavigateEvent = new NavigateEvent(NAVIGATE_TO_POINT, mapX, mapY);
                dispatchEvent(event);
            }
		}

		public function resize(width: int, height: int) : void {
			this.miniMapWidth = width;
			this.miniMapHeight = height;
            redraw();            
		}

		public function setScreenRectHidden(hidden: Boolean) : void {
			screenRect.visible = !hidden;
		}

		public function addMiniMapRegion(id:int) : MiniMapRegion
		{
			if (Constants.debug >= 2)
			Util.log("Adding city region: " + id);

			var newRegion: MiniMapRegion = new MiniMapRegion(id,mapFilter);

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

            if (Constants.debug >= 3) {
                Util.log("Parsing city regions.. Pending:" + pendingRegions.length);
            }
			
			// Remove all regions if we are forcing
			if (forceParse) {
				for each (var region: MiniMapRegion in regions) {
					region.disposeData();
					regionSpace.removeChild(region);					
				}
				
				regions.clear();
			}

			//calculate which regions we need to render
			var requiredRegions: Array = [];

			var camX: int = Global.gameContainer.camera.miniMapX - mapHolder.x;
			var camY: int = Global.gameContainer.camera.miniMapY - mapHolder.y;

			var regionsW: int = Math.ceil(miniMapWidth / Constants.miniMapRegionW);
			var regionsH: int = Math.ceil(miniMapHeight / (Constants.miniMapRegionH / 2));

			for (var c: int = 0; c <= regionsW; c++) {
				for (var r: int = 0; r <= regionsH; r++) {
					var requiredId: int = TileLocator.getMiniMapRegionId(camX + Constants.miniMapRegionW * c, camY + (Constants.miniMapRegionH / 2) * r);
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
					if (Constants.debug >= 3) {
					    Util.log("Discarded: " + region.id);
                    }

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
				if (Constants.debug >= 3) {
				    Util.log("Required city region:" + requiredRegions);
                }

				Global.mapComm.Region.getMiniMapRegion(requiredRegions);
			}
		}
		
		public function addPointer(pointer: MiniMapPointer): void {
			pointer.visible = pointersVisible;
			pointers.push(pointer);
			addChild(pointer);
		}
		
		public function setCityPointer(name: String): void {
			if (cityPointer != null && cityPointer.getPointerName() != name) {
				cityPointer.setIcon(SpriteFactory.getStarlingImage("ICON_MINIMAP_ICON_BLUE"));
			} else if (cityPointer!=null) {
				return;
			}
			
			cityPointer = Enumerable.from(pointers).first(function(p:MiniMapPointer):Boolean {
				return p.getPointerName() == name;
			});

			if (cityPointer != null) {
				cityPointer.setIcon(SpriteFactory.getStarlingImage("ICON_MINIMAP_ARROW_RED"));
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


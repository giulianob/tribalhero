package src.FeathersUI.MiniMap {

    import feathers.core.FeathersControl;

    import flash.geom.Point;
    import flash.geom.Rectangle;
    import flash.utils.Dictionary;

    import src.Constants;
    import src.Map.Camera;
    import src.Map.City;
    import src.Map.Position;
    import src.Map.ScreenPosition;
    import src.Map.TileLocator;
    import src.Objects.Factories.SpriteFactory;
    import src.UI.Components.MiniMapPointer;
    import src.Util.BinaryList.BinaryListEvent;
    import src.Util.Util;

    import starling.display.*;
    import starling.display.graphics.RoundedRectangle;
    import starling.events.Event;
    import starling.events.Touch;
    import starling.events.TouchEvent;
    import starling.events.TouchPhase;

    public class MiniMapView extends FeathersControl {
        private var pointers: Dictionary = new Dictionary();
        private var cityPointer: MiniMapPointer;

        private var mapContainer: Sprite;
        private var bg: Sprite;
        private var pointerContainer: Sprite;

        private var screenRect: Shape;
        private var screenRectWidth: Number;
        private var screenRectHeight: Number;

        private var shouldParseRegions: Boolean = true;
        private var parseRegionDirtyFlag: Boolean;

        private var _backgroundRadius: Number = 10;
        private var _scrollRate: Number = 1;

        private var vm: MiniMapVM;
        private var camera: Camera;
        private var touchMovement: Point = new Point();

        private var xStartingLoop: int;
        private var xRegionCount: Number;
        private var yRegionCount: Number;
        private var xEndingLoop: int;
        private var yStartingLoop: int;
        private var yEndingLoop: int;
        private var parseRegionPosition: Position = new Position();

        public function MiniMapView(vm: MiniMapVM) {
            this.name = "MiniMapView";
            this.vm = vm;
            this.camera = vm.camera;

            mapContainer = new Sprite();

            screenRect = new Shape();

            mapContainer.addChild(vm.objContainer);
            bg = new Sprite();
            pointerContainer = new Sprite();

            addChild(bg);
            addChild(mapContainer);
            addChild(screenRect);
            addChild(pointerContainer);

            this.addEventListener(TouchEvent.TOUCH, onTouched);
            this.camera.addEventListener(Camera.ON_MOVE, onCameraMove);
            this.vm.cities.addEventListener(BinaryListEvent.ADDED, onCityAdded);
            this.vm.cities.addEventListener(BinaryListEvent.REMOVED, onCityRemoved);

            for each (var city: City in this.vm.cities) {
                addPointer(new MiniMapPointer(city.primaryPosition.x, city.primaryPosition.y, city.name, city.id));
            }
        }

        public function get backgroundRadius(): Number {
            return _backgroundRadius;
        }

        public function set backgroundRadius(value: Number): void {
            if (_backgroundRadius == value) {
                return;
            }

            _backgroundRadius = value;
            this.invalidate(INVALIDATION_FLAG_SIZE);
        }

        private function onTouched(event: TouchEvent): void {
            var movedTouch: Touch = event.getTouch(this, TouchPhase.MOVED);

            if (movedTouch) {
                // one finger touching / one mouse cursor moved
                movedTouch.getMovement(this, touchMovement);
                camera.scrollTo(new ScreenPosition(camera.currentPosition.x - touchMovement.x*Constants.miniMapTileRatioW, camera.currentPosition.y - touchMovement.y*Constants.miniMapTileRatioH));
            }

            var endTouch:Touch = event.getTouch(this, TouchPhase.ENDED);

            if (endTouch && endTouch.tapCount == 2) {
                var touchPosition: Point = endTouch.getLocation(this);
                var camX: int = -mapContainer.x;
                var camY: int = -mapContainer.y;

                vm.navigateToPoint(new ScreenPosition((camX + touchPosition.x) * Constants.miniMapTileRatioW, (camY + touchPosition.y) * Constants.miniMapTileRatioH));
            }
        }

        override public function dispose(): void {
            super.dispose();

            this.camera.removeEventListener(Camera.ON_MOVE, onCameraMove);
            this.vm.cities.removeEventListener(BinaryListEvent.ADDED, onCityAdded);
            this.vm.cities.removeEventListener(BinaryListEvent.REMOVED, onCityRemoved);
        }

        private function onCityRemoved(event: BinaryListEvent): void {
            var city: City = event.item;

            removeCityPointer(city.id);
        }

        private function onCityAdded(event: BinaryListEvent): void {
            var city: City = event.item;
            addPointer(new MiniMapPointer(city.primaryPosition.x, city.primaryPosition.y, city.name, city.id));
        }

        private function onCameraMove(event: Event): void {
            this.invalidate(INVALIDATION_FLAG_SCROLL);
        }

        protected function layout() : void {
            var sizeInvalid:Boolean = this.isInvalid(INVALIDATION_FLAG_SIZE);
            var cameraChanged:Boolean = this.isInvalid(INVALIDATION_FLAG_SCROLL);

            if (cameraChanged || sizeInvalid) {
                var tilesW: Number = (Constants.screenW * vm.camera.getZoomFactorOverOne()) / Constants.tileW + 0.5;
                var tilesH: Number = (Constants.screenH * vm.camera.getZoomFactorOverOne()) / Constants.tileH + 0.5;
                screenRectWidth = tilesW * Constants.miniMapTileW;
                screenRectHeight = tilesH * Constants.miniMapTileH;

                mapContainer.x = -camera.miniMapX + (this.actualWidth/2 - screenRectWidth/2);
                mapContainer.y = -camera.miniMapY + (this.actualHeight/2 - screenRectHeight/2);

                mapContainer.clipRect = new Rectangle(-mapContainer.x, -mapContainer.y, this.actualWidth, this.actualHeight);

                // Redraw screen rectangle
                if (screenRectWidth < this.actualWidth && screenRectHeight < this.actualHeight) {
                    // Draw the white rectangle that represents what the player can see
                    screenRect.graphics.clear();
                    screenRect.graphics.lineStyle(1, 0xFFFFFF, 0.8);
                    screenRect.graphics.drawRect(0, 0, screenRectWidth, screenRectHeight);
                    screenRect.x = this.actualWidth/2 - screenRectWidth/2;
                    screenRect.y = this.actualHeight/2 - screenRectHeight/2;
                }
            }

            if (sizeInvalid) {
                setUpSizeVariablesForParseRegion();

                // Set up the black transparent bg
                bg.removeChildren();
                var bgRect: RoundedRectangle = new RoundedRectangle(this.actualWidth, this.actualHeight, _backgroundRadius, _backgroundRadius, _backgroundRadius, _backgroundRadius);
                bgRect.material.color = 0x000000;
                bgRect.alpha = 0.8;
                bg.addChild(bgRect);
            }

            if (cameraChanged || sizeInvalid) {
                parseRegions();
                updatePointers(camera.miniMapCenter);
            }
        }

        protected override function draw() : void {
            var needsWidth:Boolean = isNaN(this.explicitWidth);
            var needsHeight:Boolean = isNaN(this.explicitHeight);
            if(needsWidth && needsHeight)
            {
                setSizeInternal(300, 150, false);
            }

            this.layout();
        }

        public function enableRegionFetching(enable: Boolean): void {
            shouldParseRegions = enable;

            if (enable && parseRegionDirtyFlag) {
                parseRegionDirtyFlag = false;
                parseRegions();
            }
        }

        public function showScreenRect(visible: Boolean) : void {
            screenRect.visible = visible;
        }

        private function setUpSizeVariablesForParseRegion(): void {
            xRegionCount = Math.ceil(Number(this.actualWidth) / Constants.miniMapRegionW) + 1;
            yRegionCount = Math.ceil(Number(this.actualHeight) / Constants.miniMapRegionH) + 1;

            // We always want the region to be odd otherwise we would have
            // to do more complex calculations when looping below to figure out
            // whether we should have the negative side of the loop should get the extra
            // iteration or the positive
            if (xRegionCount%2==0) { xRegionCount++; }
            if (yRegionCount%2==0) { yRegionCount++; }

            // Figure out where the loop should start and end
            // We can assume xRegionCount and yRegionCount are always odd so if xRegionCount is 5 for example
            // we basically want the loop to go -2,-1,0,1,2
            xStartingLoop = int(-xRegionCount / 2.0);
            xEndingLoop = int(xRegionCount / 2.0);
            yStartingLoop = int(-yRegionCount / 2.0);
            yEndingLoop = int(yRegionCount / 2.0);
        }

        private function parseRegions(): void {
            if (!shouldParseRegions) {
                parseRegionDirtyFlag = true;
                return;
            }

            if (Constants.debug >= 3) {
                Util.log("On move: " + camera.currentPosition.x + "," + camera.currentPosition.y);
            }

            //calculate which regions we need to render
            var requiredRegions: Dictionary = new Dictionary();

            var cameraPosition: Position = camera.currentPosition.toPosition();

            for (var reqX: int = xStartingLoop; reqX <= xEndingLoop; reqX++) {
                for (var reqY: int = yStartingLoop; reqY <= yEndingLoop; reqY++) {
                    parseRegionPosition.x = cameraPosition.x + Constants.miniMapRegionTileW * reqX;
                    parseRegionPosition.y = cameraPosition.y + Constants.miniMapRegionTileH * reqY;

                    var requiredId: int = TileLocator.getMiniMapRegionId(parseRegionPosition);

                    if (requiredId > -1) {
                        requiredRegions[requiredId] = requiredId;
                    }
                }
            }

            vm.getRegions(requiredRegions);
        }

        public function addPointer(pointer: MiniMapPointer): void {
            pointers[pointer.cityId] = pointer;
            pointerContainer.addChild(pointer);
        }

        public function setActiveCityPointer(cityId: int): void {
            if (cityPointer != null) {
                if (cityPointer.cityId == cityId) {
                    return;
                }

                // reset current city pointer
                cityPointer.setIcon(SpriteFactory.getStarlingImage("ICON_MINIMAP_ARROW_BLUE"));
            }

            if (pointers[cityId] !== undefined) {
                cityPointer = pointers[cityId];
                cityPointer.setIcon(SpriteFactory.getStarlingImage("ICON_MINIMAP_ARROW_RED"));
            }
            else {
                cityPointer = null;
            }
        }

        public function removeCityPointer(cityId: int): void {
            if (cityPointer != null && cityPointer.cityId == cityId) {
                cityPointer = null;
                removeChild(cityPointer);
            }

            if (pointers[cityId] !== undefined) {
                delete pointers[cityId];
            }
        }

        public function showPointers(visible: Boolean): void {
            pointerContainer.visible = visible;
        }

        public function updatePointers(center: Point): void {
            for each(var pointer:MiniMapPointer in pointers) {
                pointer.update(center, this.actualWidth, this.actualHeight);
            }
        }

        public function get scrollRate(): Number {
            return _scrollRate;
        }

        public function set scrollRate(value: Number): void {
            _scrollRate = value;
        }
    }
}

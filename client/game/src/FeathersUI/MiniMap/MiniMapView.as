package src.FeathersUI.MiniMap {

    import feathers.core.FeathersControl;

    import flash.geom.Point;
    import flash.geom.Rectangle;
    import flash.utils.Dictionary;

    import src.Constants;
    import src.Map.Camera;
    import src.Map.Position;
    import src.Map.ScreenPosition;
    import src.Map.TileLocator;
    import src.UI.Components.MiniMapPointer;
    import src.Util.Util;

    import starling.display.*;
    import starling.display.graphics.RoundedRectangle;
    import starling.events.Event;
    import starling.events.Touch;
    import starling.events.TouchEvent;
    import starling.events.TouchPhase;

    public class MiniMapView extends FeathersControl {
        private var pointers: Array = [];
        private var pointersVisible: Boolean = false;
        private var cityPointer: MiniMapPointer;

        private var mapContainer: Sprite;
        private var bg: Sprite;

        private var screenRect: Shape;
        private var screenRectWidth: Number;
        private var screenRectHeight: Number;

        private var shouldParseRegions: Boolean = true;
        private var parseRegionDirtyFlag: Boolean;

        private var _backgroundRadius: Number = 10;

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

            addChild(bg);
            addChild(mapContainer);
            addChild(screenRect);

            this.addEventListener(TouchEvent.TOUCH, onTouched);
            this.camera.addEventListener(Camera.ON_MOVE, onCameraMove);
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
            }

            if (sizeInvalid) {
                setUpSizeVariablesForParseRegion();

                // Redraw screen rectangle
                if (screenRectWidth < this.actualWidth && screenRectHeight < this.actualHeight) {
                    // Draw the white rectangle that represents what the player can see
                    screenRect.graphics.clear();
                    screenRect.graphics.lineStyle(1, 0xFFFFFF, 0.8);
                    screenRect.graphics.drawRect(0, 0, screenRectWidth, screenRectHeight);
                    screenRect.x = this.actualWidth/2 - screenRectWidth/2;
                    screenRect.y = this.actualHeight/2 - screenRectHeight/2;
                }

                // Set up the black transparent bg
                bg.removeChildren();
                var bgRect: RoundedRectangle = new RoundedRectangle(this.actualWidth, this.actualHeight, _backgroundRadius, _backgroundRadius, _backgroundRadius, _backgroundRadius);
                bgRect.material.color = 0x000000;
                bgRect.alpha = 0.8;
                bg.addChild(bgRect);
            }

            if (cameraChanged || sizeInvalid) {
                parseRegions();
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

/*
 public function addPointer(pointer: MiniMapPointer): void {
 pointer.visible = pointersVisible;
 pointers.push(pointer);
 addChild(pointer);
 }

 public function setCityPointer(name: String): void {
 if (cityPointer != null && cityPointer.getPointerName() != name) {
 cityPointer.setIcon(SpriteFactory.getStarlingImage("ICON_MINIMAP_ARROW_BLUE"));
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
 pointer.update(center, this.actualWidth, this.actualHeight);
 }
 }

 */
    }
}

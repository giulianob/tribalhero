package src.FeathersUI.MiniMap {

    import System.Linq.Enumerable;

    import feathers.core.FeathersControl;

    import flash.geom.Point;
    import flash.geom.Rectangle;

    import src.Constants;
    import src.FeathersUI.Map.MapView;
    import src.Map.Camera;
    import src.Map.Position;
    import src.Map.ScreenPosition;
    import src.Map.TileLocator;
    import src.Objects.Factories.SpriteFactory;
    import src.UI.Components.MiniMapPointer;
    import src.Util.Util;

    import starling.display.*;
    import starling.display.graphics.RoundedRectangle;
    import starling.events.Event;
    import starling.events.Touch;
    import starling.events.TouchEvent;
    import starling.events.TouchPhase;

    public class MiniMapView extends FeathersControl {
        private var screenRect: Shape;
        private var mapContainer: Sprite;
        private var bg: Sprite;
        private var pointers: Array = [];
        private var pointersVisible: Boolean = false;
        private var cityPointer: MiniMapPointer;

        private var vm: MiniMapVM;
        private var camera: Camera;
        private var touchMovement: Point = new Point();

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

        private function onTouched(event: TouchEvent): void {
            var touches: Vector.<Touch> = event.getTouches(this, TouchPhase.MOVED);

            if (touches.length == 1) {
                // one finger touching / one mouse cursor moved
                var touch: Touch = touches[0];
                touch.getMovement(this, touchMovement);

                camera.scrollTo(new ScreenPosition(camera.currentPosition.x - touchMovement.x / mapContainer.scaleX, camera.currentPosition.y - touchMovement.y / mapContainer.scaleY));
            }
        }

        override public function dispose(): void {
            super.dispose();

            this.camera.removeEventListener(Camera.ON_MOVE, onCameraMove);
        }

        private function onCameraMove(event: Event): void {
            this.invalidate(INVALIDATION_FLAG_SCROLL);
        }

        private var screenRectWidth: Number;

        private var screenRectHeight: Number;

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

                parseRegions();
            }

            if (sizeInvalid) {
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
                var bgRect: RoundedRectangle = new RoundedRectangle(this.actualWidth, this.actualHeight);
                bgRect.material.color = 0x000000;
                bgRect.alpha = 0.8;
                bg.addChild(bgRect);
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

        public function setScreenRectHidden(hidden: Boolean) : void {
            screenRect.visible = !hidden;
        }

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

        private function parseRegions(): void {
            if (Constants.debug >= 3) Util.log("On move: " + camera.currentPosition.x + "," + camera.currentPosition.y);

            //calculate which regions we need to render
            var requiredRegions: Array = [];

            var xRegionCount: int = Math.ceil(Number(this.actualWidth) / Constants.miniMapRegionW) + 1;
            var yRegionCount: int = Math.ceil(Number(this.actualHeight) / Constants.miniMapRegionH) + 1;

            // We always want the region to be odd otherwise we would have
            // to do more complex calculations when looping below to figure out
            // whether we should have the negative side of the loop should get the extra
            // iteration or the positive
            if (xRegionCount%2==0) { xRegionCount++; }
            if (yRegionCount%2==0) { yRegionCount++; }

            var cameraPosition: Position = camera.currentPosition.toPosition();

            // Figure out where the loop should start and end
            // We can assume xRegionCount and yRegionCount are always odd so if xRegionCount is 5 for example
            // we basically want the loop to go -2,-1,0,1,2
            var xStartingLoop: int = int(-xRegionCount/2.0);
            var xEndingLoop: int = int(xRegionCount/2.0);
            var yStartingLoop: int = int(-yRegionCount/2.0);
            var yEndingLoop: int = int(yRegionCount/2.0);

            var regionPosition: Position = new Position();
            for (var reqX: int = xStartingLoop; reqX <= xEndingLoop; reqX++) {
                for (var reqY: int = yStartingLoop; reqY <= yEndingLoop; reqY++) {
                    regionPosition.x = cameraPosition.x + Constants.miniMapRegionTileW * reqX;
                    regionPosition.y = cameraPosition.y + Constants.miniMapRegionTileH * reqY;

                    var requiredId: int = TileLocator.getMiniMapRegionId(regionPosition);

                    if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1) {
                        requiredRegions.push(requiredId);
                    }
                }
            }

            vm.getRegions(requiredRegions);
        }

//        private function onNavigate(e: TouchEvent) : void {
//
//            var touch: Touch = e.getTouch(this, TouchPhase.ENDED);
//
//            if (touch != null) {
//
//                var currentMousePoint: Point = new Point(touch.globalX, touch.globalY);
//                if (Point.distance(currentMousePoint, lastClickPoint) > 10 || (new Date().time) - lastClick > 350) {
//                    lastClick = new Date().time;
//                    lastClickPoint = currentMousePoint;
//                    return;
//                }
//
//                lastClick = new Date().time;
//                //Calculate where the user clicked in real map position
//                var camX: int = _camera.miniMapX - mapContainer.x;
//                var camY: int = _camera.miniMapY - mapContainer.y;
//
//                var local: Point = touch.getLocation(this);
//                var centeredOffsetX: int = local.x;
//                var centeredOffsetY: int = local.y;
//
//                var mapX: int = ((camX + centeredOffsetX) / Constants.miniMapTileW) * Constants.tileW;
//                var mapY: int = ((camY + centeredOffsetY) / Constants.miniMapTileH) * Constants.tileH;
//
//                var event: NavigateEvent = new NavigateEvent(NAVIGATE_TO_POINT, mapX, mapY);
//                dispatchEvent(event);
//            }
//        }
    }
}

package src.FeathersUI.MiniMap {

    import System.Linq.Enumerable;

    import feathers.core.FeathersControl;

    import flash.geom.Point;
    import flash.geom.Rectangle;

    import src.Constants;
    import src.FeathersUI.Map.MapView;
    import src.Map.Camera;
    import src.Map.ScreenPosition;
    import src.Map.TileLocator;
    import src.Objects.Factories.SpriteFactory;
    import src.UI.Components.MiniMapPointer;
    import src.Util.Util;

    import starling.display.*;
    import starling.display.graphics.RoundedRectangle;
    import starling.events.Event;

    public class MiniMapView extends FeathersControl {
        private var screenRect: Shape;
        private var mapContainer: Sprite;
        private var bg: Sprite;
        private var pointers: Array = [];
        private var pointersVisible: Boolean = false;
        private var cityPointer: MiniMapPointer;

        private var vm: MiniMapVM;
        private var camera: Camera;

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

            this.camera.addEventListener(Camera.ON_MOVE, onCameraMove);
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
                var screenRectWidth: Number = tilesW * Constants.miniMapTileW;
                var screenRectHeight: Number = tilesH * Constants.miniMapTileH;

                mapContainer.x = -camera.miniMapX + (this.actualWidth/2 - screenRectWidth/2);
                mapContainer.y = -camera.miniMapY + (this.actualHeight/2 - screenRectHeight/2);

                mapContainer.clipRect = new Rectangle(-mapContainer.x, -mapContainer.y, this.actualWidth, this.actualHeight);
            }

            if (sizeInvalid) {
                parseRegions();

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

            var xRegionCount: int = Math.ceil(Number(this.actualWidth) / MapView.MIN_ZOOM / Constants.miniMapRegionW);
            var yRegionCount: int = Math.ceil(Number(this.actualHeight) / MapView.MIN_ZOOM / Constants.miniMapRegionH);

            for (var reqX: int = -1; reqX <= xRegionCount; reqX++) {
                for (var reqY: int = -1; reqY <= yRegionCount; reqY++) {
                    var screenPos: ScreenPosition = new ScreenPosition(
                                    camera.currentPosition.x + (Constants.miniMapRegionW * reqX),
                                    camera.currentPosition.y + (Constants.miniMapRegionH * reqY));

                    var requiredId: int = TileLocator.getMiniMapRegionId(screenPos.toPosition());

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

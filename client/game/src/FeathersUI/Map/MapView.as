package src.FeathersUI.Map {
    import feathers.utils.math.clamp;

    import flash.events.MouseEvent;

    import flash.geom.Point;
    import flash.geom.Rectangle;

    import src.Constants;
    import src.Global;
    import src.Map.Camera;
    import src.FeathersUI.Map.MapOverlayBase;
    import src.Map.Region;
    import src.Map.ScreenPosition;
    import src.Map.ScreenPosition;
    import src.Map.TileLocator;
    import src.Objects.*;
    import src.Util.BinaryList.BinaryListEvent;
    import src.Util.Util;

    import starling.core.Starling;

    import starling.display.*;
    import starling.events.*;

    public class MapView extends Sprite {
        public static const MIN_ZOOM: Number = 0.3;
        private static const MAX_ZOOM: Number = 1.25;

        private var mapContainer: Sprite;

        private var regionSpace: Sprite;

        public var objContainer: ObjectContainer;

        private var disabledMapQueries: Boolean;

        private var listenersDefined: Boolean;

        private var vm: MapVM;

        private var touchMovement: Point;

        public var camera: Camera;

        private var parseRegionIsDirty: Boolean;

        public function MapView(vm: MapVM) {
            this.name = "Map View";
            this.vm = vm;
            this.camera = vm.camera;
            this.listenersDefined = false;
            this.regionSpace = new Sprite();
            this.objContainer = vm.objContainer;
            this.touchMovement = new Point();

            this.vm.regions.addEventListener(BinaryListEvent.ADDED, onRegionAdded);
            this.vm.regions.addEventListener(BinaryListEvent.REMOVED, onRegionRemoved);

            addEventListener(Event.ADDED_TO_STAGE, eventAddedToStage);
            addEventListener(Event.REMOVED_FROM_STAGE, eventRemovedFromStage);
            camera.addEventListener(Camera.ON_MOVE, onMove);

            this.mapContainer = new Sprite();
            addChild(mapContainer);
            mapContainer.addChild(new MapOverlayBase());
            mapContainer.addChild(regionSpace);
            mapContainer.addChild(objContainer);
        }

        override public function dispose():void
        {
            super.dispose();

            this.camera.removeEventListener(Camera.ON_MOVE, onMove);
            this.vm.regions.removeEventListener(BinaryListEvent.ADDED, onRegionAdded);
            this.vm.regions.removeEventListener(BinaryListEvent.REMOVED, onRegionRemoved);

            disableMouse(true);
        }

        private function onRegionRemoved(event: BinaryListEvent): void {
            regionSpace.removeChild(event.item);
        }

        private function onRegionAdded(event: BinaryListEvent): void {
            var newRegion: Region = event.item;
            regionSpace.addChild(newRegion);
        }

        private function disableMouse(disable: Boolean):void
        {
            if (disable) {
                if (!listenersDefined)
                {
                    return;
                }

                mapContainer.removeEventListener(TouchEvent.TOUCH, onTouched);
                Starling.current.nativeStage.removeEventListener(MouseEvent.MOUSE_WHEEL, nativeStage_mouseWheelHandler);

                listenersDefined = false;
            }
            else {
                Global.stage.focus = Global.gameContainer;

                if (listenersDefined) {
                    return;
                }

                Starling.current.nativeStage.addEventListener(MouseEvent.MOUSE_WHEEL, nativeStage_mouseWheelHandler, false, 0, true);
                mapContainer.addEventListener(TouchEvent.TOUCH, onTouched);

                listenersDefined = true;
            }
        }

        private function nativeStage_mouseWheelHandler(event: MouseEvent): void {
            const starlingViewPort:Rectangle = Starling.current.viewPort;
            var scrollPoint: Point = new Point(
                            (event.stageX - starlingViewPort.x) / Starling.contentScaleFactor,
                            (event.stageY - starlingViewPort.y) / Starling.contentScaleFactor);

            this.globalToLocal(scrollPoint, scrollPoint);
            if (this.hitTest(scrollPoint, true))
            {
                var step: int = 10;

                var newValue: Number;

                if (event.delta > 0) {
                     newValue = camera.zoomFactor + step;
                }

                if (event.delta < 0) {
                    newValue = camera.zoomFactor - step;
                }

                var center: ScreenPosition = camera.mapCenter();

                camera.beginMove();
                camera.zoomFactor = clamp(newValue, MIN_ZOOM*100, MAX_ZOOM*100);
                camera.scrollToCenter(center);
                camera.endMove();
            }
        }

        private function onTouched(event: TouchEvent): void {
            var touches:Vector.<Touch> = event.getTouches(this, TouchPhase.MOVED);

            if (touches.length == 1)
            {
                // one finger touching / one mouse cursor moved
                var touch:Touch = touches[0];
                touch.getMovement(this, touchMovement);

                camera.scrollTo(new ScreenPosition(camera.currentPosition.x - touchMovement.x/mapContainer.scaleX, camera.currentPosition.y - touchMovement.y/mapContainer.scaleY));
            }
            else if (touches.length == 2) {
                // two fingers touching -> zoom in
                var touchA: Touch = touches[0];
                var touchB: Touch = touches[1];

                var currentPosA:Point  = touchA.getLocation(this);
                var previousPosA:Point = touchA.getPreviousLocation(this);
                var currentPosB:Point  = touchB.getLocation(this);
                var previousPosB:Point = touchB.getPreviousLocation(this);

                var currentVector:Point  = currentPosA.subtract(currentPosB);
                var previousVector:Point = previousPosA.subtract(previousPosB);

                var sizeDiff:Number = currentVector.length / previousVector.length;

                var previousLocalA:Point  = touchA.getPreviousLocation(mapContainer);
                var previousLocalB:Point  = touchB.getPreviousLocation(mapContainer);
                var pivotX: Number = (previousLocalA.x + previousLocalB.x) * 0.5;
                var pivotY: Number = (previousLocalA.y + previousLocalB.y) * 0.5;

                mapContainer.x = (currentPosA.x + currentPosB.x) * 0.5;
                mapContainer.y = (currentPosA.y + currentPosB.y) * 0.5;
                mapContainer.scaleX = mapContainer.scaleY = clamp(mapContainer.scaleY*sizeDiff, MIN_ZOOM, MAX_ZOOM);
                mapContainer.pivotX = pivotX;
                mapContainer.pivotY = pivotY;

                // normalize point
                var result: Point = new Point();
                mapContainer.localToGlobal(new Point(0,0), result);
                camera.beginMove();
                camera.scrollTo(new ScreenPosition(-result.x/mapContainer.scaleX, -result.y/mapContainer.scaleY));
                camera.zoomFactor = mapContainer.scaleX*100.0;
                camera.endMove();
            }

        }

        public function disableMapQueries(disabled: Boolean) : void {
            objContainer.disableMouse(disabled);
            disabledMapQueries = disabled;

            if (!disabled && parseRegionIsDirty) {
                parseRegionIsDirty = false;
                update();
            }
        }

        public function eventAddedToStage(event: Event):void
        {
            disableMouse(false);

            update();
        }

        public function eventRemovedFromStage(event: Event):void
        {
            disableMouse(true);
        }

        public function update(updatePositionFromCamera: Boolean = true): void {
            if (updatePositionFromCamera && !disabledMapQueries) {
                mapContainer.pivotX = 0;
                mapContainer.pivotY = 0;
                mapContainer.x = -camera.currentPosition.getXAsNumber() * camera.getZoomFactorPercentage();
                mapContainer.y = -camera.currentPosition.getYAsNumber() * camera.getZoomFactorPercentage();
                mapContainer.scaleX = mapContainer.scaleY = camera.getZoomFactorPercentage();
            }

            parseRegions();
        }

        private function onMove(event: Event) : void
        {
            update();
        }

        private function parseRegions(): void {
            if (disabledMapQueries) {
                parseRegionIsDirty = true;
                return;
            }

            if (Constants.debug >= 3) Util.log("On move: " + camera.currentPosition.x + "," + camera.currentPosition.y);

            //calculate which regions we need to render
            var requiredRegions: Array = [];

            var xRegionCount: int = Math.ceil(Number(Constants.screenW) / MIN_ZOOM / Constants.regionW);
            var yRegionCount: int = Math.ceil(Number(Constants.screenH) / MIN_ZOOM / Constants.regionH);

            for (var reqX: int = -1; reqX <= xRegionCount; reqX++) {
                for (var reqY: int = -1; reqY <= yRegionCount; reqY++) {
                    var screenPos: ScreenPosition = new ScreenPosition(
                                    camera.currentPosition.x + (Constants.regionW * reqX),
                                    camera.currentPosition.y + (Constants.regionH * reqY));

                    var requiredId: int = TileLocator.getRegionId(screenPos.toPosition());

                    if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1) {
                        requiredRegions.push(requiredId);
                    }
                }
            }

            vm.getRegions(requiredRegions);
        }
    }
}

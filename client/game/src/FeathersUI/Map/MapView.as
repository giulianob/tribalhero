package src.FeathersUI.Map {
    import flash.events.MouseEvent;
    import flash.geom.Point;
    import flash.geom.Rectangle;

    import src.Constants;
    import src.Global;
    import src.Map.Camera;
    import src.Map.MapOverlayBase;
    import src.Map.Position;
    import src.Map.Region;
    import src.Map.ScreenPosition;
    import src.Map.TileLocator;
    import src.Objects.*;
    import src.Util.BinaryList.BinaryListEvent;
    import src.Util.Util;

    import starling.display.*;
    import starling.events.*;
    import starling.utils.formatString;

    public class MapView extends Sprite {
        private var regionSpace: Sprite;

        public var objContainer: ObjectContainer;

        private var mouseDown: Boolean;

        private var mouseLoc: Point;

        private var originPoint: Point = new Point();

        private var disabledMapQueries: Boolean;

        private var listenersDefined: Boolean;

        private var vm: MapVM;

        private var touchMovement: Point;

        public var camera: Camera;

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

            addChild(new MapOverlayBase());
            addChild(regionSpace);
            addChild(objContainer);
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
            newRegion.moveWithCamera(camera);
            regionSpace.addChild(newRegion);
        }

        public function disableMouse(disable: Boolean):void
        {
            if (disable) {
                if (!listenersDefined)
                {
                    return;
                }

//                    Global.stage.removeEventListener(MouseEvent.MOUSE_DOWN, eventMouseDown);
//                    Global.stage.removeEventListener(MouseEvent.MOUSE_MOVE, eventMouseMove);
//                    Global.stage.removeEventListener(MouseEvent.MOUSE_UP, eventMouseUp);
//                    Global.stage.removeEventListener(flash.events.Event.MOUSE_LEAVE, eventMouseLeave);

                listenersDefined = false;
            }
            else {
                Global.stage.focus = Global.gameContainer;

                if (listenersDefined) {
                    return;
                }

                addEventListener(TouchEvent.TOUCH, onTouched);
//                    Global.stage.addEventListener(MouseEvent.MOUSE_DOWN, eventMouseDown);
//                    Global.stage.addEventListener(MouseEvent.MOUSE_MOVE, eventMouseMove);
//                    Global.stage.addEventListener(MouseEvent.MOUSE_UP, eventMouseUp);
//                    Global.stage.addEventListener(flash.events.Event.MOUSE_LEAVE, eventMouseLeave);

                listenersDefined = true;
            }
        }

        private function onTouched(event: TouchEvent): void {
            var touches:Vector.<Touch> = event.getTouches(this, TouchPhase.MOVED);

            if (touches.length == 1)
            {
                trace("Single touch");
                // one finger touching / one mouse curser moved
                var touch:Touch = touches[0];
                touch.getMovement(this, touchMovement);

                camera.move(-touchMovement.x, -touchMovement.y);
            }
            else if (touches.length == 2) {
                trace("Multi touch");

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

                var touchAMovement: Point = touchA.getMovement(this);
                var touchBMovement: Point = touchB.getMovement(this);

                var prevZoomFactor: int = camera.zoomFactor;

                camera.beginMove();
                camera.zoomFactor *= sizeDiff;

                trace("currentVector", currentVector.length, "previousVector", previousVector.length, "sizeDiff", sizeDiff, "prevZoomFactor",prevZoomFactor, "newZoomFactor", camera.zoomFactor);
                camera.move(-(touchAMovement.x + touchBMovement.x) * 0.5, -(touchAMovement.y + touchBMovement.y) * 0.5);
                camera.endMove();


                trace(formatString("sizeDiff: {0} zoomFactor: {1} currentVector: {2} previousVector: {3}", sizeDiff, camera.zoomFactor, currentVector.length, previousVector.length));

                //camera.scrollRate = gameContainer.camera.getZoomFactorOverOne();
                //camera.scrollToCenter(center);
            }
        }

        public function disableMapQueries(disabled: Boolean) : void {
            objContainer.disableMouse(disabled);
            disabledMapQueries = disabled;
        }

        public function eventMouseDown(event: MouseEvent):void
        {
            originPoint = new Point(event.stageX, event.stageY);
            mouseLoc = new Point(event.stageX, event.stageY);
            mouseDown = true;

            if (Constants.debug >= 4)
                Util.log("MOUSE DOWN");
        }

        public function eventMouseUp(event: MouseEvent):void
        {
            mouseDown = false;

            if (Point.distance(new Point(event.stageX, event.stageY), originPoint) < 4) {
                vm.doSelectedObject(null);
            }

            if (Constants.debug >= 4)
                Util.log("MOUSE UP");
        }

        public function eventMouseLeave(event: flash.events.Event):void
        {
            mouseDown = false;

            if (Constants.debug >= 4)
                Util.log("MOUSE LEAVE");
        }

        public function eventMouseMove(event: MouseEvent):void
        {
            if (!mouseDown) {
                if (event.shiftKey) {
                    var screenMouse: Point = TileLocator.getPointWithZoomFactor(event.stageX, event.stageY);
                    var mapPixelPos: ScreenPosition = TileLocator.getActualCoord(camera.currentPosition.x + screenMouse.x, camera.currentPosition.y + screenMouse.y);
                    var mapPos: Position = mapPixelPos.toPosition();
                    Global.gameContainer.setLabelCoords(mapPos);
                }

                return;
            }

            var dx: Number = (mouseLoc.x - event.stageX) * camera.scrollRate * Constants.scale;
            var dy: Number = (mouseLoc.y - event.stageY) * camera.scrollRate * Constants.scale;

            if (Math.abs(dx) < 1 && Math.abs(dy) < 1) return;

            mouseLoc = new Point(event.stageX, event.stageY);

            camera.move(dx, dy);
        }

        public function eventAddedToStage(event: Event):void
        {
            disableMouse(false);
        }

        public function eventRemovedFromStage(event: Event):void
        {
            disableMouse(true);
        }

        public function move(): void {
            scaleX = scaleY = camera.getZoomFactorPercentage();

            var pt: Position = camera.mapCenter().toPosition();

            Global.gameContainer.setLabelCoords(pt);

            if (!disabledMapQueries) {
                parseRegions();
                objContainer.moveWithCamera(camera.currentPosition.x, camera.currentPosition.y);
            }
        }

        public function onMove(event: flash.events.Event) : void
        {
            move();
        }

        public function parseRegions(): void {
            if (Constants.debug >= 3) Util.log("On move: " + camera.currentPosition.x + "," + camera.currentPosition.y);

            //calculate which regions we need to render
            var requiredRegions: Array = [];

            // Get list of required regions
            const offset: int = 200;

            var screenRect: Rectangle = new Rectangle(
                            camera.currentPosition.x - offset, camera.currentPosition.y - offset,
                            Constants.screenW * camera.getZoomFactorOverOne() + offset * 2.0, Constants.screenH * camera.getZoomFactorOverOne() + offset * 2.0);

            for (var reqX: int = -1; reqX <= Math.ceil((Constants.screenW * camera.getZoomFactorOverOne()) / Constants.regionW); reqX++) {
                for (var reqY: int = -1; reqY <= Math.ceil((Constants.screenH * camera.getZoomFactorOverOne()) / (Constants.regionH / 2)); reqY++) {
                    var screenPos: ScreenPosition = new ScreenPosition(camera.currentPosition.x + (Constants.regionW * reqX),
                                    camera.currentPosition.y + (Constants.regionH / 2 * reqY));
                    var requiredId: int = TileLocator.getRegionId(screenPos);

                    var regionRect: Rectangle = TileLocator.getRegionRect(requiredId);
                    if (!regionRect.containsRect(screenRect) && !screenRect.intersects(regionRect)) continue;

                    if (requiredId > -1 && requiredRegions.indexOf(requiredId) == -1) requiredRegions.push(requiredId);
                }
            }

            vm.getRegions(requiredRegions);

            for (var i: int = vm.regions.size() - 1; i >= 0; i--) {
                vm.regions[i].moveWithCamera(camera);
            }
        }
    }
}

package src {
    import com.codecatalyst.promise.Deferred;
    import com.codecatalyst.promise.Promise;

    import feathers.system.DeviceCapabilities;

    import flash.display.Stage;
    import flash.geom.Point;
    import flash.geom.Rectangle;

    import src.Global;
    import src.Util.Util;

    import starling.core.Starling;
    import starling.display.Sprite;
    import starling.events.Event;
    import starling.utils.AssetManager;

    public class StarlingStage extends Sprite {

        public static var _baseWidth:Number = 960;
        public static var _baseHeight:Number = 640;

        public static var assets: AssetManager = new AssetManager();

        public function StarlingStage() {
        }

        public static function init(stage: Stage): Promise {
            Starling.handleLostContext = true;
            Starling.multitouchEnabled = true;

            var stageInitDeferred: Deferred = new Deferred();
            var queueInitDeferred: Deferred = new Deferred();

            var starling: Starling = new Starling(StarlingStage, stage);
            starling.start();
            starling.addEventListener(Event.ROOT_CREATED, function(e: Event): void {
                trace("Starling init complete. Mode is " + Starling.current.context.driverInfo);

                Global.starlingStage = StarlingStage(e.data);

                updateViewport(Starling.current.nativeStage.stageWidth, Starling.current.nativeStage.stageHeight);

                stage.addEventListener(Event.RESIZE, onResizeStage);

                stageInitDeferred.resolve(null);
            });

            stageInitDeferred.promise.then(function(): void {
                assets.enqueue(StarlingAssets);
                assets.loadQueue(function(ratio:Number):void
                {
                    if (ratio == 1.0) {
                        queueInitDeferred.resolve(null);
                    }
                });
            });

            return queueInitDeferred.promise;
        }

        private static function updateViewport($width:Number, $height:Number):void
        {
            // Resize the Starling viewport with the new width and height
            Starling.current.viewPort = new Rectangle(0, 0, $width, $height);

            if (!Starling.current && !Starling.current.stage) {
                return;
            }

            if (DeviceCapabilities.isPhone(Starling.current.nativeStage)) {
                Util.log("Running in phone mode");

                // Get the scale based on the biggest percentage between the new width and the base width or the new height and the base height
                var scale:Number = Math.max(( $width / _baseWidth ), ( $height / _baseHeight ));

                // Resize the starling stage based on the new width and height divided by the scale
                Starling.current.stage.stageWidth = $width / scale;
                Starling.current.stage.stageHeight = $height / scale;
            }
            else {
                Util.log("Running as desktop");
                Starling.current.stage.stageWidth = $width;
                Starling.current.stage.stageHeight = $height;
            }
        }

        private static function onResizeStage(e: *):void
        {
            updateViewport(Starling.current.nativeStage.stageWidth, Starling.current.nativeStage.stageHeight);

            CONFIG::debug {
                Starling.current.showStatsAt("right", "bottom");
            }
        }
    }
}

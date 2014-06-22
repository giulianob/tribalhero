package src {
    import com.codecatalyst.promise.Deferred;
    import com.codecatalyst.promise.Promise;

    import flash.display.Stage;
    import flash.geom.Point;
    import flash.geom.Rectangle;

    import starling.core.Starling;
    import starling.display.Sprite;
    import starling.events.Event;
    import starling.utils.AssetManager;

    public class StarlingStage extends Sprite {

        public static var assets: AssetManager = new AssetManager();

        public function StarlingStage() {
        }

        public static function init(stage: Stage): Promise {
            Starling.handleLostContext = true;

            var stageInitDeferred: Deferred = new Deferred();
            var _starling: Starling = new Starling(StarlingStage, stage);
            _starling.start();
            _starling.addEventListener(Event.ROOT_CREATED, function(e: Event): void {
                trace("Starling init complete. Mode is " + Starling.current.context.driverInfo);

                Global.starlingStage = StarlingStage(e.data);

                CONFIG::debug {
                    Starling.current.showStats = true;
                    Starling.current.showStatsAt("right", "bottom");
                }

                stage.addEventListener(Event.RESIZE, onResizeStage);

                stageInitDeferred.resolve(null);
            });

            assets.enqueue(StarlingAssets);

            var queueInitDeferred: Deferred = new Deferred();
            assets.loadQueue(function(ratio:Number):void
            {
                if (ratio == 1.0) {
                    queueInitDeferred.resolve(null);
                }
            });

            return Promise.all([queueInitDeferred.promise, stageInitDeferred.promise]);
        }

        private static function onResizeStage(e: *):void
        {
            var viewPortRectangle:Rectangle = new Rectangle();
            viewPortRectangle.width = Starling.current.nativeStage.stageWidth;
            viewPortRectangle.height = Starling.current.nativeStage.stageHeight;

            Starling.current.viewPort = viewPortRectangle;
            Starling.current.stage.stageWidth = viewPortRectangle.width;
            Starling.current.stage.stageHeight = viewPortRectangle.height;

            Starling.current.showStatsAt("right", "bottom");
        }
    }
}

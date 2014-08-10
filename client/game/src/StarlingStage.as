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
            var queueInitDeferred: Deferred = new Deferred();

            var starling: Starling = new Starling(StarlingStage, stage);
            starling.start();
            starling.addEventListener(Event.ROOT_CREATED, function(e: Event): void {
                trace("Starling init complete. Mode is " + Starling.current.context.driverInfo);

                Global.starlingStage = StarlingStage(e.data);

                CONFIG::debug {
                    Starling.current.showStats = true;
                    Starling.current.showStatsAt("right", "bottom");
                }

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

        private static function onResizeStage(e: *):void
        {
            var viewPortRectangle:Rectangle = new Rectangle();
            viewPortRectangle.width = Starling.current.nativeStage.stageWidth;
            viewPortRectangle.height = Starling.current.nativeStage.stageHeight;

            Starling.current.viewPort = viewPortRectangle;
            Starling.current.stage.stageWidth = viewPortRectangle.width;
            Starling.current.stage.stageHeight = viewPortRectangle.height;

            CONFIG::debug {
                Starling.current.showStatsAt("right", "bottom");
            }
        }
    }
}

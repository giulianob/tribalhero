package src {
    import com.codecatalyst.promise.Deferred;
    import com.codecatalyst.promise.Promise;

    import feathers.controls.ScreenNavigator;
    import feathers.motion.transitions.ScreenSlidingStackTransitionManager;

    import flash.display.Stage;

    import src.Graphics.Tileset;

    import starling.core.Starling;
    import starling.display.Image;
    import starling.display.Sprite;
    import starling.events.Event;
    import starling.textures.Texture;
    import starling.utils.AssetManager;

    public class StarlingStage extends Sprite {
        private static var _bootstrapper: IBootstrapper;
        public var assets: AssetManager;
        private static var stageInitDeferred: Deferred;

        public var navigator: ScreenNavigator;

        public function StarlingStage() {
            super();

            navigator = new ScreenNavigator();
            new ScreenSlidingStackTransitionManager(navigator);

            addChild(navigator);

            this.addEventListener(Event.ADDED_TO_STAGE, addedToStage);
        }

        public static function init(stage: Stage, bootstrapper: IBootstrapper): Promise {
            _bootstrapper = bootstrapper;

            stageInitDeferred = new Deferred();

            new Starling(StarlingStage, stage).start();

            return stageInitDeferred.promise;
        }

        private function addedToStage(event: Event): void {
            trace("Starling init complete. Mode is " + Starling.current.context.driverInfo);

            Global.starlingStage = this;

            _bootstrapper.init(Starling.current);

            onResizeStage(null);

            stage.addEventListener(Event.RESIZE, onResizeStage);

            assets = _bootstrapper.loadAssets(Starling.current);

            assets.enqueue(Assets);

            assets.loadQueue(function(ratio:Number):void
            {
                if (ratio == 1.0) {
                    stageInitDeferred.resolve(null);
                }
            });
        }

        private function onResizeStage(e: *):void
        {
            _bootstrapper.updateViewport(Starling.current, Starling.current.nativeStage.stageWidth, Starling.current.nativeStage.stageHeight);

            Constants.screenW = Starling.current.stage.stageWidth;
            Constants.screenH = Starling.current.stage.stageHeight;
        }
    }
}

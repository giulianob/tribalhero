package src {
    import com.codecatalyst.promise.Deferred;
    import com.codecatalyst.promise.Promise;

    import feathers.controls.ScreenNavigator;

    import feathers.system.DeviceCapabilities;
    import feathers.themes.MetalWorksDesktopTheme;
    import feathers.themes.MetalWorksMobileTheme;

    import flash.display.Stage;
    import flash.geom.Rectangle;

    import src.Util.Util;

    import starling.core.Starling;
    import starling.display.Sprite;
    import starling.events.Event;
    import starling.utils.AssetManager;

    public class StarlingStage extends Sprite {
        public static var _baseWidth:Number = 960;
        public static var _baseHeight:Number = 640;
        public static var assets: AssetManager = new AssetManager();
        private static var stageInitDeferred: Deferred;

        public var navigator: ScreenNavigator = new ScreenNavigator();

        public function StarlingStage() {
            super();

            addChild(navigator);

            this.addEventListener(Event.ADDED_TO_STAGE, addedToStage);
        }

        private function addedToStage(event: Event): void {
            trace("Starling init complete. Mode is " + Starling.current.context.driverInfo);

            Global.starlingStage = this;

            updateViewport(Starling.current.nativeStage.stageWidth, Starling.current.nativeStage.stageHeight);

            if (DeviceCapabilities.isPhone(Starling.current.nativeStage) || DeviceCapabilities.isPhone(Starling.current.nativeStage)) {
                new MetalWorksMobileTheme();
            }
            else {
                new MetalWorksDesktopTheme();
            }

            stage.addEventListener(Event.RESIZE, onResizeStage);

            assets.enqueue(StarlingAssets);
            assets.loadQueue(function(ratio:Number):void
            {
                if (ratio == 1.0) {
                    stageInitDeferred.resolve(null);
                }
            });
        }

        public static function init(stage: Stage): Promise {
            Starling.handleLostContext = true;
            Starling.multitouchEnabled = true;

            stageInitDeferred = new Deferred();

            var starling: Starling = new Starling(StarlingStage, stage);
            starling.simulateMultitouch = CONFIG::debug;

            starling.start();

            return stageInitDeferred.promise;
        }

        private static function updateViewport($width:Number, $height:Number):void
        {
            // Resize the Starling viewport with the new width and height
            Starling.current.viewPort = new Rectangle(0, 0, $width, $height);

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

            trace("DPI: " + DeviceCapabilities.dpi);
            trace("Screen Inches: " + DeviceCapabilities.screenInchesX(Starling.current.nativeStage) + "x" + DeviceCapabilities.screenInchesY(Starling.current.nativeStage));
            trace("Screen Pixels: " + DeviceCapabilities.screenPixelWidth + DeviceCapabilities.screenPixelHeight);
        }

        private static function onResizeStage(e: *):void
        {
            updateViewport(Starling.current.nativeStage.stageWidth, Starling.current.nativeStage.stageHeight);

            CONFIG::debug {
                //Starling.current.showStatsAt("right", "bottom");
            }
        }
    }
}

package src {
    import feathers.system.DeviceCapabilities;
    import feathers.themes.MetalWorksMobileTheme;

    import flash.filesystem.File;
    import flash.geom.Rectangle;
    import flash.system.Capabilities;

    import src.FeathersUI.Factories.IFlowFactory;
    import src.FeathersUI.Factories.MobileFlowFactory;
    import src.FeathersUI.Map.MapVM;
    import src.FeathersUI.MiniMap.MiniMapVM;
    import src.Map.MiniMap.MiniMap;

    import starling.core.Starling;
    import starling.utils.AssetManager;
    import starling.utils.formatString;

    public class MobileBootstrapper implements IBootstrapper {
        private static const dpiRatiosAllowed: Array = [0.5, 0.75, 1, 1.5, 2];
        private static const assetSizes: Array = [1, 1.5, 2];

        public function MobileBootstrapper() {
            Starling.handleLostContext = true;
            Starling.multitouchEnabled = true;
        }

        public function init(stage: Starling): void {
            stage.simulateMultitouch = CONFIG::debug;

            Constants.initMapSize(0.5);
        }

        public function updateViewport(starling: Starling, screenWidth: Number, screenHeight: Number): void {
            // Viewport is always the full screen size
            starling.viewPort = new Rectangle(0, 0, screenWidth, screenHeight);

            var dpiRatio: Number = DeviceCapabilities.dpi / 163.0;

            // might not need it
            // dpiRatio = findScaleFactor(dpiRatio, dpiRatiosAllowed);

            starling.stage.stageWidth = Math.floor(screenWidth / dpiRatio);
            starling.stage.stageHeight = Math.floor(screenHeight / dpiRatio);

            trace("DpiRatio: "  + dpiRatio);
            trace("Screen size: " + screenWidth + "x" + screenHeight);
            trace("Stage size: " + starling.stage.stageWidth + "x" + starling.stage.stageHeight);
            trace("Starling ScaleFactor: "  + starling.contentScaleFactor);
            trace("DPI: " + DeviceCapabilities.dpi);
            trace("Screen Inches: " + DeviceCapabilities.screenInchesX(starling.nativeStage) + "x" + DeviceCapabilities.screenInchesY(starling.nativeStage));
        }

        public function loadAssets(starling: Starling): AssetManager {
            new MetalWorksMobileTheme();

            var appDir:File = File.applicationDirectory;
            var assetScaleFactor: Number = findScaleFactor(starling.contentScaleFactor, assetSizes);
            //Constants.updateContentScale(assetScaleFactor);

            trace(formatString("Loading assets at {0}x scale", assetScaleFactor));

            var assets: AssetManager = new AssetManager(assetScaleFactor);
            assets.verbose = CONFIG::debug;
            assets.enqueue(
                    appDir.resolvePath(formatString("atlas/{0}x", assetScaleFactor))
            );

            return assets;
        }

        protected function findScaleFactor(scaleFactor: Number, allowedFactors: Array):Number
        {
            var scaleF:Number = Math.floor(scaleFactor * 1000) / 1000;
            var closest:Number = 0;
            var f:Number;

            for each (f in allowedFactors) {
                if (closest === 0 || Math.abs(f - scaleF) < Math.abs(closest - scaleF)) {
                    closest = f;
                }
            }

            return closest;
        }

        public function getFlowFactory(mapVM: MapVM, miniMapVM: MiniMapVM): IFlowFactory {
            return new MobileFlowFactory(mapVM, miniMapVM);
        }
    }
}

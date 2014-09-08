package src {
    import feathers.system.DeviceCapabilities;
    import feathers.themes.MetalWorksMobileTheme;

    import flash.filesystem.File;
    import flash.geom.Rectangle;
    import flash.system.Capabilities;

    import starling.core.Starling;
    import starling.utils.AssetManager;
    import starling.utils.formatString;

    public class MobileBootstrapper implements IBootstrapper {
        private static const dpiRatiosAllowed: Array = [0.5, 0.75, 1, 1.5, 2];
        private static const assetSizes: Array = [0.5, 0.75, 1];

        public function MobileBootstrapper() {
            var iOS:Boolean = Capabilities.manufacturer.indexOf("iOS") != -1;
            Starling.handleLostContext = !iOS;
            Starling.multitouchEnabled = true;
        }

        public function init(stage: Starling): void {
            stage.simulateMultitouch = CONFIG::debug;
        }

        public function updateViewport(starling: Starling, screenWidth: Number, screenHeight: Number): void {
            // Viewport is always the full screen size
            starling.viewPort = new Rectangle(0, 0, screenWidth, screenHeight);

            /*
            // Get the scale based on the biggest percentage between the new width and the base width or the new height and the base height
            var scale:Number = Math.max(( width / baseWidth ), ( height / baseHeight ));

            // Resize the starling stage based on the new width and height divided by the scale
             starling.stage.stageWidth = width / scale;
             starling.stage.stageHeight = height / scale;
            */

            /*
            Citrus Engine based scaling
            var baseRect:Rectangle = new Rectangle(0, 0, _baseWidth, _baseHeight);
            var screenRect:Rectangle = new Rectangle(0, 0, screenWidth, screenHeight);

            var viewport: Rectangle = RectangleUtil.fit(baseRect, screenRect, ScaleMode.SHOW_ALL);
            var viewportBaseRatioWidth: Number = viewport.width / baseRect.width;
            var viewportBaseRatioHeight: Number = viewport.height / baseRect.height;

             starling.stage.stageWidth = screenRect.width / viewportBaseRatioWidth;
             starling.stage.stageHeight = screenRect.height / viewportBaseRatioHeight;
            */

            /*
            const baseDpi: int = 326;
            var dpiRatio: Number = DeviceCapabilities.dpi / baseDpi;
            starling.stage.stageWidth = Math.floor(screenWidth / dpiRatio);
            starling.stage.stageHeight = Math.floor(screenHeight / dpiRatio);
            */

            var dpiRatio: Number = DeviceCapabilities.dpi / 326.0;
            dpiRatio = findScaleFactor(dpiRatio, dpiRatiosAllowed);

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
    }
}

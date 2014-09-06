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
        private static const assetSizes: Array = [1, 1.5, 2];

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

            const baseDpi: int = 163;
            var dpiRatio: Number = DeviceCapabilities.dpi / baseDpi;
            starling.stage.stageWidth = Math.floor(screenWidth / dpiRatio);
            starling.stage.stageHeight = Math.floor(screenHeight / dpiRatio);

            trace("DpiRatio: "  + dpiRatio);
            trace("Starling ScaleFactor: "  + starling.contentScaleFactor);
            trace("DPI: " + DeviceCapabilities.dpi);
            trace("Screen Inches: " + DeviceCapabilities.screenInchesX(starling.nativeStage) + "x" + DeviceCapabilities.screenInchesY(starling.nativeStage));
        }

        public function loadAssets(starling: Starling): AssetManager {
            new MetalWorksMobileTheme();

            var appDir:File = File.applicationDirectory;

            var assets: AssetManager = new AssetManager();
            assets.verbose = CONFIG::debug;

            // Gfx that are used to render the map (structures/tiles/walls/etc..) dont get HD variants
            // since we allow zooming in stead
            assets.scaleFactor = 1;
            assets.enqueue(
                    appDir.resolvePath("atlas/noscale")
            );

            // Gfx like icons and UI will scale
            var assetScaleFactor: Number = findScaleFactor(starling);
            assets.scaleFactor = assetScaleFactor;
            assets.enqueue(
                    appDir.resolvePath(formatString("atlas/{0}x", assetScaleFactor))
            );

            return assets;
        }

        protected function findScaleFactor(starling: Starling):Number
        {
            var scaleF:Number = Math.floor(starling.contentScaleFactor * 1000) / 1000;
            var closest:Number = 0;
            var f:Number;

            for each (f in assetSizes) {
                if (closest === 0 || Math.abs(f - scaleF) < Math.abs(closest - scaleF)) {
                    closest = f;
                }
            }

            return closest;
        }
    }
}

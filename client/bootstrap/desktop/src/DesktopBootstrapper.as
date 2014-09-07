package src {
    import feathers.system.DeviceCapabilities;
    import feathers.themes.MetalWorksDesktopTheme;

    import flash.geom.Rectangle;

    import starling.core.Starling;
    import starling.utils.AssetManager;

    public class DesktopBootstrapper implements IBootstrapper {

        public function DesktopBootstrapper() {
            Starling.handleLostContext = true;
        }

        public function init(stage: Starling): void {
        }

        public function updateViewport(stage: Starling, width: Number, height: Number): void
        {
            // Resize the Starling viewport with the new width and height
            Starling.current.viewPort = new Rectangle(0, 0, width, height);

            Starling.current.stage.stageWidth = width;
            Starling.current.stage.stageHeight = height;

            trace("DPI: " + DeviceCapabilities.dpi);
            trace("Screen Inches: " + DeviceCapabilities.screenInchesX(Starling.current.nativeStage) + "x" + DeviceCapabilities.screenInchesY(Starling.current.nativeStage));
        }

        public function loadAssets(stage: Starling): AssetManager {
            new MetalWorksDesktopTheme();

            var assets: AssetManager = new AssetManager();
            assets.enqueue(DesktopAssets);

            return assets;
        }
    }
}

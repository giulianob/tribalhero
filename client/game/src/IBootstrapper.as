package src {
    import starling.core.Starling;
    import starling.utils.AssetManager;

    public interface IBootstrapper {
        function updateViewport(starling: Starling, width: Number, height: Number): void;

        function loadAssets(starling: Starling): AssetManager;

        function init(starling: Starling): void;
    }
}

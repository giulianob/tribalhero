package src {
    import src.FeathersUI.Factories.IFlowFactory;
    import src.Map.Map;
    import src.Map.MiniMap.MiniMap;

    import starling.core.Starling;
    import starling.utils.AssetManager;

    public interface IBootstrapper {
        function updateViewport(starling: Starling, width: Number, height: Number): void;

        function loadAssets(starling: Starling): AssetManager;

        function init(starling: Starling): void;

        function getFlowFactory(map: Map, miniMap: MiniMap): IFlowFactory;
    }
}

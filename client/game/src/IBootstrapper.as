package src {
    import src.FeathersUI.Factories.IFlowFactory;
    import src.FeathersUI.Map.MapVM;
    import src.FeathersUI.MiniMap.MiniMapVM;

    import starling.core.Starling;
    import starling.utils.AssetManager;

    public interface IBootstrapper {
        function updateViewport(starling: Starling, width: Number, height: Number): void;

        function loadAssets(starling: Starling): AssetManager;

        function init(starling: Starling): void;

        function getFlowFactory(mapVM: MapVM, miniMapVM: MiniMapVM): IFlowFactory;
    }
}

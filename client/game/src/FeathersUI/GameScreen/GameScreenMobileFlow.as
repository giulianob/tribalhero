package src.FeathersUI.GameScreen {
    import feathers.controls.ScreenNavigatorItem;

    import src.FeathersUI.Flow;
    import src.FeathersUI.Map.MapVM;
    import src.FeathersUI.Map.MapView;
    import src.FeathersUI.MiniMap.MiniMapVM;
    import src.FeathersUI.MiniMap.MiniMapView;
    import src.FeathersUI.MiniMap.WorldMapMobileVM;
    import src.FeathersUI.MiniMap.WorldMapMobileView;
    import src.Global;

    public class GameScreenMobileFlow extends Flow implements IGameScreenFlow {
        private var mapVM: MapVM;
        private var miniMapVM: MiniMapVM;

        public function GameScreenMobileFlow(map: MapVM, minimap: MiniMapVM) {
            mapVM = map;
            miniMapVM = minimap;
        }

        public function show(): void {
            mapVM.camera.scrollToCenter(mapVM.cities[0].primaryPosition.toScreenPosition());

            // Main game screen setup
            var mapView: MapView = new MapView(mapVM);
            var gameContainerVm: GameScreenVM = new GameScreenVM(mapVM.cities);
            var gameContainerView: GameScreenMobileView = new GameScreenMobileView(gameContainerVm, mapView);

            var gameContainerViewTransitions: Object = {};
            gameContainerViewTransitions[GameScreenMobileView.EVENT_VIEW_MINIMAP] = "worldMapView"; // Show minimap when button is pressed
            Global.starlingStage.navigator.addScreen("gameContainer", new ScreenNavigatorItem(gameContainerView, gameContainerViewTransitions));

            // World map screen setup
            var miniMapView: MiniMapView = new MiniMapView(miniMapVM);

            var worldMapMobileVM: WorldMapMobileVM = new WorldMapMobileVM();
            var worldMapMobileView: WorldMapMobileView = new WorldMapMobileView(worldMapMobileVM, miniMapView);

            var worldMapMobileViewTransitions: Object = {};
            worldMapMobileViewTransitions[WorldMapMobileView.EVENT_CLOSE_MINIMAP] = "gameContainer"; // Return to gameContainer when minimap is closed
            Global.starlingStage.navigator.addScreen("worldMapView", new ScreenNavigatorItem(worldMapMobileView, worldMapMobileViewTransitions));

            // Show game screen
            Global.starlingStage.navigator.showScreen("gameContainer");
        }
    }
}

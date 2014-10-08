package src.FeathersUI.GameScreen {
    import feathers.controls.ScreenNavigatorItem;

    import src.FeathersUI.Flow;
    import src.FeathersUI.Map.MapVM;
    import src.FeathersUI.Map.MapView;
    import src.FeathersUI.MiniMap.MiniMapVM;
    import src.FeathersUI.MiniMap.MiniMapView;
    import src.FeathersUI.MiniMap.WorldMapMobileVM;
    import src.FeathersUI.MiniMap.WorldMapMobileView;
    import src.FeathersUI.ViewModelEvent;
    import src.Global;

    import starling.events.Event;

    public class GameScreenMobileFlow extends Flow implements IGameScreenFlow {
        private var mapVM: MapVM;
        private var miniMapVM: MiniMapVM;

        private var mapView: MapView;
        private var miniMapView: MiniMapView;

        public function GameScreenMobileFlow(map: MapVM, minimap: MiniMapVM) {
            mapVM = map;
            miniMapVM = minimap;
        }

        public function show(): void {
            mapVM.camera.scrollToCenter(mapVM.cities[0].primaryPosition.toScreenPosition());

            miniMapVM.addEventListener(MiniMapVM.EVENT_NAVIGATE_TO_POINT, onMiniMapNavigateToPoint);

            // Main game screen setup
            mapView = new MapView(mapVM);
            var gameContainerVm: GameScreenVM = new GameScreenVM(mapVM.cities);
            var gameContainerView: GameScreenMobileView = new GameScreenMobileView(gameContainerVm, mapView);
            gameContainerView.addEventListener(GameScreenMobileView.EVENT_VIEW_MINIMAP, onViewMinimap);

            var gameContainerViewTransitions: Object = {};
            gameContainerViewTransitions[GameScreenMobileView.EVENT_VIEW_MINIMAP] = "worldMapView"; // Show minimap when button is pressed
            Global.starlingStage.navigator.addScreen("gameContainer", new ScreenNavigatorItem(gameContainerView, gameContainerViewTransitions));

            // World map screen setup
            miniMapView = new MiniMapView(miniMapVM);

            var worldMapMobileVM: WorldMapMobileVM = new WorldMapMobileVM();
            var worldMapMobileView: WorldMapMobileView = new WorldMapMobileView(worldMapMobileVM, miniMapView, miniMapVM.mapFilter);
            worldMapMobileView.addEventListener(WorldMapMobileView.EVENT_CLOSE_MINIMAP, onCloseMinimap);

            var worldMapMobileViewTransitions: Object = {};
            worldMapMobileViewTransitions[WorldMapMobileView.EVENT_CLOSE_MINIMAP] = "gameContainer"; // Return to gameContainer when minimap is closed
            Global.starlingStage.navigator.addScreen("worldMapView", new ScreenNavigatorItem(worldMapMobileView, worldMapMobileViewTransitions));

            // Show game screen
            Global.starlingStage.navigator.showScreen("gameContainer");
        }

        private function onMiniMapNavigateToPoint(): void {
            Global.starlingStage.navigator.showScreen("gameContainer");

            mapView.disableMapQueries(false);
        }

        private function onCloseMinimap(event: Event): void {
            mapVM.camera.goToCue();

            mapView.disableMapQueries(false);
        }

        private function onViewMinimap(event: Event): void {
            mapVM.camera.cue();

            mapView.disableMapQueries(true);
        }
    }
}
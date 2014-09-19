package src.FeathersUI.GameScreen {
    import feathers.controls.ScreenNavigatorItem;

    import src.FeathersUI.Flow;
    import src.FeathersUI.Map.MapVM;
    import src.FeathersUI.Map.MapView;
    import src.FeathersUI.MiniMap.MiniMapVM;
    import src.FeathersUI.MiniMap.MiniMapView;
    import src.Global;
    import src.Map.MiniMap.MiniMap;

    public class GameScreenDesktopFlow extends Flow implements IGameScreenFlow {
        private var mapVM: MapVM;
        private var miniMapVM: MiniMapVM;

        public function GameScreenDesktopFlow(mapVM: MapVM, miniMapVM: MiniMapVM) {
            this.mapVM = mapVM;
            this.miniMapVM = miniMapVM;
        }

        public function show(): void {
            mapVM.camera.scrollToCenter(mapVM.cities[0].primaryPosition.toScreenPosition());

            var miniMapView: MiniMapView = new MiniMapView(miniMapVM);
            var mapView: MapView = new MapView(mapVM);
            var gameContainerVm: GameScreenVM = new GameScreenVM(mapVM.cities);
            var gameContainerView: GameScreenDesktopView = new GameScreenDesktopView(gameContainerVm, mapView, miniMapView);

            Global.starlingStage.navigator.addScreen("gameContainer", new ScreenNavigatorItem(gameContainerView));
            Global.starlingStage.navigator.showScreen("gameContainer");
        }
    }
}

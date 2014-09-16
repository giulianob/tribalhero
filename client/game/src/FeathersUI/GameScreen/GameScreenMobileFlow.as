package src.FeathersUI.GameScreen {
    import feathers.controls.ScreenNavigatorItem;

    import src.FeathersUI.Flow;
    import src.FeathersUI.Map.MapVM;
    import src.FeathersUI.Map.MapView;
    import src.Global;
    import src.Map.MiniMap.MiniMap;

    public class GameScreenMobileFlow extends Flow implements IGameScreenFlow {
        private var mapVM: MapVM;
        private var miniMap: MiniMap;

        public function GameScreenMobileFlow(map: MapVM, minimap: MiniMap) {
            mapVM = map;
            miniMap = minimap;
        }

        public function show(): void {
            mapVM.camera.scrollToCenter(mapVM.cities[0].primaryPosition.toScreenPosition());

            var mapView: MapView = new MapView(mapVM);
            var gameContainerVm: GameScreenVM = new GameScreenVM(mapVM.cities);
            var gameContainerView: GameScreenMobileView = new GameScreenMobileView(gameContainerVm, mapView, miniMap);

            Global.starlingStage.navigator.addScreen("gameContainer", new ScreenNavigatorItem(gameContainerView));
            Global.starlingStage.navigator.showScreen("gameContainer");
        }
    }
}

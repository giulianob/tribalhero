package src.FeathersUI.GameScreen {
    import feathers.controls.ScreenNavigatorItem;

    import src.FeathersUI.Flow;
    import src.FeathersUI.Map.MapVM;
    import src.FeathersUI.Map.MapView;
    import src.Global;
    import src.Map.MiniMap.MiniMap;

    public class GameScreenDesktopFlow extends Flow implements IGameScreenFlow {
        private var mapVM: MapVM;
        private var miniMap: MiniMap;

        public function GameScreenDesktopFlow(mapVM: MapVM, miniMap: MiniMap) {
            this.mapVM = mapVM;
            this.miniMap = miniMap;
        }

        public function show(): void {
            mapVM.camera.scrollToCenter(mapVM.cities[0].primaryPosition.toScreenPosition());

            var mapView: MapView = new MapView(mapVM);
            var gameContainerVm: GameScreenVM = new GameScreenVM(mapVM.cities);
            var gameContainerView: GameScreenDesktopView = new GameScreenDesktopView(gameContainerVm, mapView, miniMap);

            Global.starlingStage.navigator.addScreen("gameContainer", new ScreenNavigatorItem(gameContainerView));
            Global.starlingStage.navigator.showScreen("gameContainer");
        }
    }
}

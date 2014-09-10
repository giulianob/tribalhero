package src.FeathersUI.GameScreen {
    import feathers.controls.ScreenNavigatorItem;

    import src.FeathersUI.Flow;
    import src.Global;
    import src.Map.Map;
    import src.Map.MiniMap.MiniMap;

    public class GameScreenMobileFlow extends Flow implements IGameScreenFlow {
        private var _map: Map;
        private var _minimap: MiniMap;

        public function GameScreenMobileFlow(map: Map, minimap: MiniMap) {
            _map = map;
            _minimap = minimap;
        }

        public function show(): void {
            _map.camera.scrollToCenter(_map.cities[0].primaryPosition.toScreenPosition());

            var gameContainerVm: GameScreenVM = new GameScreenVM(_map, _minimap);
            var gameContainerView: GameScreenMobileView = new GameScreenMobileView(gameContainerVm);

            Global.starlingStage.navigator.addScreen("gameContainer", new ScreenNavigatorItem(gameContainerView));
            Global.starlingStage.navigator.showScreen("gameContainer");
        }
    }
}

package src.FeathersUI.Factories {
    import src.FeathersUI.GameScreen.GameScreenDesktopFlow;
    import src.FeathersUI.GameScreen.IGameScreenFlow;
    import src.Map.Map;
    import src.Map.MiniMap.MiniMap;

    public class DesktopFlowFactory implements IFlowFactory {
        private var map: Map;
        private var miniMap: MiniMap;

        public function DesktopFlowFactory(map: Map, miniMap: MiniMap) {
            this.map = map;
            this.miniMap = miniMap;
        }

        public function createGameScreenFlow(): IGameScreenFlow {
            return new GameScreenDesktopFlow(map, miniMap);
        }
    }
}

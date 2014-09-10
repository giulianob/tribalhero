package src.FeathersUI.Factories {
    import src.FeathersUI.GameScreen.GameScreenMobileFlow;
    import src.FeathersUI.GameScreen.IGameScreenFlow;
    import src.Map.Map;
    import src.Map.MiniMap.MiniMap;

    public class MobileFlowFactory implements IFlowFactory {
        private var map: Map;
        private var miniMap: MiniMap;

        public function MobileFlowFactory(map: Map, miniMap: MiniMap) {
            this.map = map;
            this.miniMap = miniMap;
        }

        public function createGameScreenFlow(): IGameScreenFlow {
            return new GameScreenMobileFlow(map, miniMap);
        }
    }
}

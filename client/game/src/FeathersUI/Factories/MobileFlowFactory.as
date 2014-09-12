package src.FeathersUI.Factories {
    import src.FeathersUI.GameScreen.GameScreenMobileFlow;
    import src.FeathersUI.GameScreen.IGameScreenFlow;
    import src.FeathersUI.Map.MapVM;
    import src.Map.MiniMap.MiniMap;

    public class MobileFlowFactory implements IFlowFactory {
        private var map: MapVM;
        private var miniMap: MiniMap;

        public function MobileFlowFactory(map: MapVM, miniMap: MiniMap) {
            this.map = map;
            this.miniMap = miniMap;
        }

        public function createGameScreenFlow(): IGameScreenFlow {
            return new GameScreenMobileFlow(map, miniMap);
        }
    }
}

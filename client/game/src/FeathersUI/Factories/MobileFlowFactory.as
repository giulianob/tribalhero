package src.FeathersUI.Factories {
    import src.FeathersUI.GameScreen.GameScreenMobileFlow;
    import src.FeathersUI.GameScreen.IGameScreenFlow;
    import src.FeathersUI.Map.MapVM;
    import src.FeathersUI.MiniMap.MiniMapVM;
    import src.Map.MiniMap.MiniMap;

    public class MobileFlowFactory implements IFlowFactory {
        private var mapVM: MapVM;
        private var miniMapVM: MiniMapVM;

        public function MobileFlowFactory(mapVM: MapVM, miniMapVM: MiniMapVM) {
            this.mapVM = mapVM;
            this.miniMapVM = miniMapVM;
        }

        public function createGameScreenFlow(): IGameScreenFlow {
            return new GameScreenMobileFlow(mapVM, miniMapVM);
        }
    }
}

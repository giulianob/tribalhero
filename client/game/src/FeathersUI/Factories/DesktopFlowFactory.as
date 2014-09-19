package src.FeathersUI.Factories {
    import src.FeathersUI.GameScreen.GameScreenDesktopFlow;
    import src.FeathersUI.GameScreen.IGameScreenFlow;
    import src.FeathersUI.Map.MapVM;
    import src.FeathersUI.MiniMap.MiniMapVM;
    import src.Map.MiniMap.MiniMap;

    public class DesktopFlowFactory implements IFlowFactory {
        private var mapVM: MapVM;
        private var miniMapVM: MiniMapVM;

        public function DesktopFlowFactory(mapVM: MapVM, miniMapVM: MiniMapVM) {
            this.mapVM = mapVM;
            this.miniMapVM = miniMapVM;
        }

        public function createGameScreenFlow(): IGameScreenFlow {
            return new GameScreenDesktopFlow(mapVM, miniMapVM);
        }
    }
}

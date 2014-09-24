package src.FeathersUI.Factories {
    import src.FeathersUI.GameScreen.IGameScreenFlow;

    public interface IFlowFactory {
        function createGameScreenFlow(): IGameScreenFlow;
    }
}

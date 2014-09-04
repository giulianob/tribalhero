package src.FeathersUI.GameScreen {
    import src.Map.Map;
    import src.Map.MiniMap.MiniMap;

    public class GameScreenVM {
        private var _map: Map;
        private var _minimap: MiniMap;

        public function GameScreenVM(map: Map, minimap: MiniMap) {
            _map = map;
            _minimap = minimap;
        }

        public function get map(): Map {
            return _map;
        }

        public function get minimap(): MiniMap {
            return _minimap;
        }
    }
}

package src.FeathersUI.GameScreen {
    import src.Map.CityList;

    public class GameScreenVM {
        private var _cities: CityList;

        public function GameScreenVM(cities: CityList) {
            this._cities = cities;
        }

        public function get cities(): CityList {
            return _cities;
        }
    }
}

package src.FeathersUI.GameScreen {
    import src.FeathersUI.ViewModel;
    import src.Map.CityList;

    public class GameScreenVM extends ViewModel {
        private var _cities: CityList;

        public function GameScreenVM(cities: CityList) {
            this._cities = cities;
        }

        public function get cities(): CityList {
            return _cities;
        }
    }
}

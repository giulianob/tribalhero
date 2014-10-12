package src.FeathersUI.GameScreen {
    import src.FeathersUI.ViewModel;
    import src.Map.City;
    import src.Map.CityList;

    public class GameScreenVM extends ViewModel {
        public static const EVENT_SELECTED_CITY_CHANGED: String = "EVENT_SELECTED_CITY_CHANGED";

        private var _cities: CityList;
        private var _selectedCity: City;

        public function GameScreenVM(cities: CityList) {
            this._cities = cities;

            if (cities.length > 0) {
                _selectedCity = cities.getByIndex(0);
            }
        }

        public function get cities(): CityList {
            return _cities;
        }

        public function get selectedCity(): City {
            return _selectedCity;
        }

        public function set selectedCity(value: City): void {
            _selectedCity = value;

            dispatchWith(EVENT_SELECTED_CITY_CHANGED);
        }
    }
}

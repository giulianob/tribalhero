package src.FeathersUI.GameScreen {
    import src.FeathersUI.ViewModel;
    import src.Global;
    import src.Map.Camera;
    import src.Map.City;
    import src.Map.CityList;

    public class GameScreenVM extends ViewModel {
        public static const EVENT_SELECTED_CITY_CHANGED: String = "EVENT_SELECTED_CITY_CHANGED";

        private var _cities: CityList;
        private var _selectedCity: City;
        private var camera: Camera;

        public function GameScreenVM(camera: Camera, cities: CityList) {
            this.camera = camera;
            this._cities = cities;

            if (cities.length > 0) {
                selectedCity = cities.getByIndex(0);
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

            Global.selectedCity = value;
            dispatchWith(EVENT_SELECTED_CITY_CHANGED);
        }

        public function zoomToSelectedCity(): void {
            if (!selectedCity) {
                return;
            }

            camera.scrollToCenter(selectedCity.primaryPosition.toTileCenteredScreenPosition());
        }
    }
}

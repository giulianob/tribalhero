package src.FeathersUI.Controls {
    import feathers.controls.Label;

    public class CityLabel extends Label {
        public function CityLabel(cityId: int, cityName: String = null, showTooltip: Boolean = true) {
            this.text = cityId.toString();
        }
    }
}

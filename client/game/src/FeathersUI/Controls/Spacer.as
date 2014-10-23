package src.FeathersUI.Controls {
    import feathers.core.FeathersControl;

    public class Spacer extends FeathersControl {
        private var _width: int;
        private var _height: int;

        public function Spacer(width: int = 1, height: int = 1) {
            _width = width;
            _height = height;

            this.width = width;
            this.height = height;
        }
    }
}

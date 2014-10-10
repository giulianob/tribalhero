package src {
    public class DeviceCapabilities {
        private var _isTouch: Boolean;

        public function DeviceCapabilities(isTouch: Boolean) {
            this._isTouch = isTouch;
        }

        public function get isTouch(): Boolean {
            return _isTouch;
        }
    }
}

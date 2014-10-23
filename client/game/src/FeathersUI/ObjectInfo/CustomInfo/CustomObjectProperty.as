package src.FeathersUI.ObjectInfo.CustomInfo {
    public class CustomObjectProperty {
        private var _name: String;
        private var _value: String;
        private var _icon: String;
        private var _tooltip: String;

        public function CustomObjectProperty(name: String, value: String, icon: String = null, tooltip: String = null) {
            this._name = name;
            this._value = value;
            this._icon = icon;
            this._tooltip = tooltip;
        }

        public function get name(): String {
            return _name;
        }

        public function get value(): String {
            return _value;
        }

        public function get icon(): String {
            return _icon;
        }

        public function get tooltip(): String {
            return _tooltip;
        }
    }
}

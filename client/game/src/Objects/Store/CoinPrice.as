package src.Objects.Store {
    public class CoinPrice {
        private var _price: int;
        private var _coins: int;
        private var _name: String;
        private var _discount: int;

        public function CoinPrice(name: String, price: int, coins: int, discount: int) {
            _name = name;
            _price = price;
            _coins = coins;
            _discount = discount;
        }

        public function get price(): int {
            return _price;
        }

        public function get coins(): int {
            return _coins;
        }

        public function get name(): String {
            return _name;
        }

        public function get discount(): int {
            return _discount;
        }
    }
}

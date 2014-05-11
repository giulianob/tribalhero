package src.UI.ViewModels {
    import com.codecatalyst.promise.Deferred;
    import com.codecatalyst.promise.Promise;

    import flash.events.Event;

    import src.Constants;
    import src.SessionVariables;

    import src.Util.Util;

    public class StoreBuyCoinsVM {
        public var itemCost: int;
        private var _purchasedCoinsDeferred: Deferred;

        public function StoreBuyCoinsVM(itemCost: int) {
            this._purchasedCoinsDeferred = new Deferred();
            this.itemCost = itemCost;

            Constants.session.addEventListener(SessionVariables.COINS_UPDATE, onCoinsUpdate, false, 0, true);
        }

        private function onCoinsUpdate(event: Event): void {
            _purchasedCoinsDeferred.resolve(Constants.session.coins);
        }

        public function buy(refillPackage: String): void {
            Util.triggerJavascriptEvent('clientBuyCoins', refillPackage);
        }

        public function get purchasedCoinsPromise(): Promise {
            return _purchasedCoinsDeferred.promise;
        }

        public function dispose(): void {
            Constants.session.removeEventListener(SessionVariables.COINS_UPDATE, onCoinsUpdate);
        }
    }
}

package src.UI.ViewModels {
    import com.codecatalyst.promise.Deferred;

    import flash.events.Event;

    import src.Constants;
    import src.SessionVariables;
    import src.FeathersUI.ViewModel;
    import src.Util.Util;

    public class StoreBuyCoinsVM extends ViewModel {
        public static const EVENT_COINS_PURCHASED: String = "EVENT_COINS_PURCHASED";
        public static const EVENT_ITEM_COST_COINS_PURCHASED: String = "EVENT_ITEM_COST_COINS_PURCHASED";

        public var itemCost: int;

        public function StoreBuyCoinsVM(itemCost: int = 0) {
            this.itemCost = itemCost;

            Constants.session.addEventListener(SessionVariables.COINS_UPDATE, onCoinsUpdate, false, 0, true);
        }

        private function onCoinsUpdate(event: Event): void {
            dispatchWith(EVENT_COINS_PURCHASED);

            if (Constants.session.coins >= itemCost) {
                dispatchWith(EVENT_ITEM_COST_COINS_PURCHASED);
            }
        }

        public function buy(refillPackage: String): void {
            Util.triggerJavascriptEvent('clientBuyCoins', refillPackage);
        }

        public function dispose(): void {
            Constants.session.removeEventListener(SessionVariables.COINS_UPDATE, onCoinsUpdate);
        }
    }
}

package src.UI.ViewModels {
    import src.Util.Util;

    public class StoreBuyCoinsVM {
        public var itemCost: int;

        public function StoreBuyCoinsVM(itemCost: int) {
            this.itemCost = itemCost;
        }

        public function buy(refillPackage: String): void {
            Util.triggerJavascriptEvent('clientBuyCoins', refillPackage)
        }
    }
}

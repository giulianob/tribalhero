package src.Objects.Process {
    import com.codecatalyst.promise.Deferred;

    import src.Global;
    import src.Objects.Theme;
    import src.UI.Dialog.StoreViewThemeDetails;

    public class PurchaseStoreThemeProcess {
        private var theme: Theme;

        public function PurchaseStoreThemeProcess(theme: Theme) {
            this.theme = theme;
        }

        public function execute(): void {
            var itemDetails: StoreViewThemeDetails = new StoreViewThemeDetails(theme);

            itemDetails.getBuyItemPromise()
                    .then(function(): void {
                        itemDetails.getFrame().dispose();

                        if (needsMoreCoins) {
                            return new BuyCoinsProcess(confirmationDialog).execute();
                        }
                        else {
                            return new Deferred().resolve(null);
                        }
                    })
                    .then(function() {
                        var confirmationDialog: StoreBuyConfirmationDialog = new StoreBuyConfirmationDialog(theme);
                        confirmationDialog.show();
                        return confirmationDialog.getConfirmBuyPromise();
                    })
                    .then(function() {
                        return Global.mapComm.Store.buyItem(theme.id);
                    })
                    .always(function(): void {
                        // show theme details again whether user purchases or not
                        execute();
                    })
                    .done();

            itemDetails.show();
        }

        private function buyMoreCoins(): void {

        }
    }
}

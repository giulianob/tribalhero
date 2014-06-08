package src.UI.Dialog {
    import com.codecatalyst.promise.Deferred;
    import com.codecatalyst.promise.Promise;

    import org.aswing.AsWingConstants;
    import org.aswing.BorderLayout;
    import org.aswing.GridLayout;
    import org.aswing.Insets;
    import org.aswing.JButton;
    import org.aswing.JFrame;
    import org.aswing.JLabel;
    import org.aswing.JPanel;
    import org.aswing.SoftBoxLayout;
    import org.aswing.UIManager;
    import org.aswing.border.EmptyBorder;
    import org.aswing.ext.Form;
    import org.aswing.ext.MultilineLabel;

    import src.Constants;
    import src.Global;
    import src.Objects.Store.CoinPrice;
    import src.UI.Components.CoinLabel;
    import src.UI.Dialog.StoreConfirmBuyDialog;
    import src.UI.GameJPanel;
    import src.UI.LookAndFeel.GameLookAndFeel;
    import src.UI.LookAndFeel.GamePanelBackgroundDecorator;
    import src.UI.ViewModels.StoreBuyCoinsVM;
    import src.Util.StringHelper;

    public class StoreBuyCoinsDialog extends GameJPanel {
        private var viewModel: StoreBuyCoinsVM;
        private var purchaseThemeDeferred: Deferred;

        public function StoreBuyCoinsDialog(viewModel: StoreBuyCoinsVM) {
            this.viewModel = viewModel;
            this.purchaseThemeDeferred = new Deferred();

            createUI();

            viewModel.on(StoreBuyCoinsVM.EVENT_COINS_PURCHASED, function(): void {
                createUI();
            });
        }

        private function createUI(): void {
            removeAll();

            title = t("STORE_BUY_COINS_DIALOG_TITLE");

            setPreferredWidth(600);
            setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 10));

            var formBalance: Form = new Form();
            formBalance.addRow(new JLabel(StringHelper.localize("STORE_BUY_COINS_DIALOG_BALANCE"), null, AsWingConstants.LEFT),
                               new CoinLabel(Constants.session.coins));

            if (viewModel.itemCost > 0) {
                append(new MultilineLabel(StringHelper.localize("STORE_BUY_COINS_DIALOG_DESCRIPTION"), 0, 50));

                formBalance.addRow(new JLabel(StringHelper.localize("STORE_BUY_COINS_DIALOG_PURCHASE_COST"), null, AsWingConstants.LEFT),
                        new CoinLabel(viewModel.itemCost));

                if (Constants.session.coins < viewModel.itemCost) {
                    formBalance.addRow(new JLabel(StringHelper.localize("STORE_BUY_COINS_DIALOG_NEEDED"), null, AsWingConstants.LEFT),
                        new CoinLabel(viewModel.itemCost - Constants.session.coins));
                }
            }

            append(formBalance);

            var pricesPanel: JPanel = new JPanel(new GridLayout(1, 3, 20));
            for each (var coinPrice: CoinPrice in Constants.coinPrices) {
                pricesPanel.append(createRefillItem(coinPrice.name, coinPrice.price, coinPrice.coins, coinPrice.discount));
            }

            append(pricesPanel);
        }

        private function createRefillItem(refillPackage: String, cost: int, coins: int, discount: int): JPanel
        {
            var wrapper: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));

            var lblCoins: CoinLabel = new CoinLabel(coins);
            lblCoins.setHorizontalAlignment(AsWingConstants.CENTER);
            GameLookAndFeel.changeClass(lblCoins, "darkSectionHeader");

            var btnBuy: JButton = new JButton(StringHelper.localize("STORE_BUY_COINS_DIALOG_REFILL_PRICE", cost));

            wrapper.appendAll(lblCoins, btnBuy);

            if (discount > 0) {
                var lblDiscount: JLabel = new JLabel(StringHelper.localize("STORE_BUY_COINS_DIALOG_REFILL_DISCOUNT", discount));
                wrapper.append(lblDiscount);
            }

            wrapper.setBackgroundDecorator(new GamePanelBackgroundDecorator("TabbedPane.top.contentRoundImage"));
            wrapper.setBorder(new EmptyBorder(null, UIManager.get("TabbedPane.contentMargin") as Insets));

            var localRefRefillPackage: String = refillPackage;
            btnBuy.addActionListener(function(): void {
                viewModel.buy(localRefRefillPackage);
            });

            wrapper.pack();

            return wrapper;
        }

        public function show(owner:* = null, modal:Boolean = true, onClose:Function = null):JFrame
        {
            super.showSelf(owner, modal, onClose, function(): void {
                viewModel.dispose();
            });

            frame.setResizable(false);
            frame.pack();

            Global.gameContainer.showFrame(frame);

            return frame;
        }
    }
}

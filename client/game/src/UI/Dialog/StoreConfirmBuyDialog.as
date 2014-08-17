package src.UI.Dialog {
    import flash.events.Event;

    import org.aswing.AsWingConstants;
    import org.aswing.CenterLayout;
    import org.aswing.JButton;
    import org.aswing.JFrame;
    import org.aswing.JLabel;
    import org.aswing.JPanel;
    import org.aswing.SoftBoxLayout;
    import org.aswing.ext.Form;
    import org.aswing.ext.MultilineLabel;

    import src.Constants;
    import src.Global;
    import src.UI.Components.CoinLabel;
    import src.UI.GameJPanel;
    import src.UI.ViewModels.StoreConfirmBuyVM;

    public class StoreConfirmBuyDialog extends GameJPanel {

        private var viewModel: StoreConfirmBuyVM;

        private var btnConfirm: JButton;

        public function StoreConfirmBuyDialog(viewModel: StoreConfirmBuyVM) {
            this.viewModel = viewModel;

            createUI();

            btnConfirm.addActionListener(function(e: Event): void {
               viewModel.buyItem();
            });
        }

        private function createUI(): void {
            setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 10));

            title = viewModel.item.localizedName;

            btnConfirm = new JButton(t("STORE_CONFIRM_DIALOG_CONFIRM"));

            var formBalance: Form = new Form();
            formBalance.addRow(new JLabel(t("STORE_CONFIRM_DIALOG_CURRENT_BALANCE"), null, AsWingConstants.LEFT), new CoinLabel(Constants.session.coins));
            formBalance.addRow(new JLabel(t("STORE_CONFIRM_DIALOG_ITEM_COST", viewModel.item.localizedName), null, AsWingConstants.LEFT), new CoinLabel(viewModel.item.cost));
            formBalance.addRow(new JLabel(t("STORE_CONFIRM_DIALOG_REMAINING_BALANCE"), null, AsWingConstants.LEFT), new CoinLabel(Constants.session.coins - viewModel.item.cost));

            var buttons: JPanel = new JPanel(new CenterLayout());
            buttons.append(btnConfirm);

            appendAll(new MultilineLabel(t("STORE_CONFIRM_DIALOG_DESCRIPTION", viewModel.item.localizedName), 0, 50),
                      formBalance,
                      buttons);
        }

        public function show(owner:* = null, modal:Boolean = true, onClose:Function = null):JFrame
        {
            super.showSelf(owner, modal, onClose, null);

            frame.setResizable(false);
            frame.pack();

            Global.gameContainer.showFrame(frame);

            return frame;
        }
    }
}

package src.UI.Dialog {
    import org.aswing.JFrame;
    import org.aswing.JPanel;

    import src.Global;
    import src.UI.GameJPanel;
    import src.UI.ViewModels.StoreConfirmBuyVM;

    public class StoreConfirmBuyDialog extends GameJPanel {
        private var viewModel: StoreConfirmBuyVM;

        public function StoreConfirmBuyDialog(viewModel: StoreConfirmBuyVM) {
            this.viewModel = viewModel;
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

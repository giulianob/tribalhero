package src.UI.Dialog {
    import org.aswing.AsWingConstants;
    import org.aswing.AssetIcon;
    import org.aswing.FlowLayout;
    import org.aswing.JButton;
    import org.aswing.JFrame;
    import org.aswing.JLabel;
    import org.aswing.JPanel;
    import org.aswing.SoftBoxLayout;
    import org.aswing.ext.MultilineLabel;

    import src.Assets;

    import src.Global;
    import src.UI.GameJPanel;
    import src.UI.LookAndFeel.GameLookAndFeel;
    import src.UI.ViewModels.StoreViewAchievementDetailsVM;

    public class StoreViewAchievementDetailsDialog extends GameJPanel {
        private var viewModel: StoreViewAchievementDetailsVM;
        private var btnBuy: JButton;
        private var lblDescription: MultilineLabel;

        public function StoreViewAchievementDetailsDialog(viewModel: StoreViewAchievementDetailsVM) {
            this.viewModel = viewModel;
            this.title = viewModel.achievement.localizedName;

            createUI();

            btnBuy.addActionListener(function(): void {
                viewModel.buy();
            });
        }

        private function createUI(): void {
            setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 10));
            setPreferredWidth(540);

            btnBuy = new JButton(t("STORE_VIEW_ACHIEVEMENT_DIALOG_BUY", viewModel.achievement.cost), new AssetIcon(Assets.getInstance("ICON_COIN")));
            btnBuy.setHorizontalTextPosition(AsWingConstants.LEFT);
            btnBuy.setIconTextGap(0);

            var pnlBuyRow: JPanel = new JPanel(new FlowLayout(AsWingConstants.RIGHT, 0, 0, false));
            pnlBuyRow.appendAll(btnBuy);

            var lblTitle: JLabel = new JLabel(viewModel.achievement.localizedName, new AssetIcon(viewModel.achievement.thumbnail()), AsWingConstants.LEFT);
            GameLookAndFeel.changeClass(lblTitle, "darkHeader");

            lblDescription = new MultilineLabel(viewModel.achievement.localizedDescription, 0, 100);

            appendAll(lblTitle, lblDescription, new JLabel(t("STORE_VIEW_ACHIEVEMENT_DIALOG_MULTIPLE_PURCHASES"), null, AsWingConstants.RIGHT), pnlBuyRow);

            lblDescription.pack();
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

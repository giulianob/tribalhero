package src.UI.Components {
    import org.aswing.AsWingConstants;
    import org.aswing.AssetIcon;
    import org.aswing.JLabel;

    import src.Assets;

    public class CoinLabel extends JLabel {
        public function CoinLabel(amount: int) {
            super(amount.toString(), new AssetIcon(Assets.getInstance("ICON_COIN")), AsWingConstants.RIGHT);
            setHorizontalTextPosition(JLabel.LEFT);
            setIconTextGap(0);
        }
    }
}

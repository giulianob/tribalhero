package src.UI.Tooltips {
    import org.aswing.AsWingConstants;
    import org.aswing.AssetIcon;
    import org.aswing.JLabel;

    import src.Objects.Store.IStoreAsset;
    import src.UI.LookAndFeel.GameLookAndFeel;

    public class StoreItemTooltip extends Tooltip {
        private var item: IStoreAsset;

        public function StoreItemTooltip(item: IStoreAsset) {
            this.item = item;

            createUI();
        }

        private function createUI(): void {
            var lblTheme: JLabel = new JLabel(item.title(), new AssetIcon(item.thumbnail()));
            GameLookAndFeel.changeClass(lblTheme, "Tooltip.text");
            lblTheme.setHorizontalTextPosition(AsWingConstants.CENTER);
            lblTheme.setVerticalTextPosition(AsWingConstants.BOTTOM);
            ui.append(lblTheme);
        }
    }
}

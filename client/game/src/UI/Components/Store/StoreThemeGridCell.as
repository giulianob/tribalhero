package src.UI.Components.Store {
    import flash.display.Bitmap;

    import org.aswing.ASColor;

    import org.aswing.AsWingConstants;
    import org.aswing.AsWingUtils;
    import org.aswing.AssetIcon;
    import org.aswing.AssetPane;
    import org.aswing.BorderLayout;
    import org.aswing.CenterLayout;
    import org.aswing.Component;
    import org.aswing.Insets;
    import org.aswing.JLabel;
    import org.aswing.JPanel;
    import org.aswing.SoftBoxLayout;
    import org.aswing.UIManager;
    import org.aswing.border.EmptyBorder;
    import org.aswing.border.LineBorder;
    import org.aswing.ext.GridList;
    import org.aswing.ext.GridListCell;

    import src.Assets;
    import src.Objects.Theme;
    import src.UI.Components.CoinLabel;
    import src.UI.LookAndFeel.GameLookAndFeel;
    import src.UI.LookAndFeel.GamePanelBackgroundDecorator;
    import src.Util.StringHelper;
    import src.Util.Util;

    public class StoreThemeGridCell implements GridListCell {
        private var component: JPanel = new JPanel();
        private var theme: Theme;

        public function setGridListCellStatus(gridList: GridList, selected: Boolean, index: int): void {
        }

        public function setCellValue(value: *): void {
            this.theme = Theme(value);

            var thumbnailImage: Bitmap = Assets.getInstance(theme.id + "_THEME_THUMBNAIL");
            Util.resizeSprite(thumbnailImage, 140, 70);

            var thumbnail: AssetPane = new AssetPane(thumbnailImage);

            var lblName: JLabel = new JLabel(theme.localizedName, null, AsWingConstants.LEFT);
            lblName.setConstraints("Center");
            GameLookAndFeel.changeClass(lblName, "darkSmallHeader");

            var lblCost: CoinLabel = new CoinLabel(theme.cost);
            GameLookAndFeel.changeClass(lblCost, "darkSmallHeader");
            lblCost.setConstraints("East");

            var footer: JPanel = new JPanel(new BorderLayout(5));
            footer.setConstraints("South");
            footer.appendAll(lblName, lblCost);

            component.useHandCursor = true;
            component.mouseEnabled = true;
            component.mouseChildren = true;
            component.setBackgroundDecorator(new GamePanelBackgroundDecorator("TabbedPane.top.contentRoundImage"));
            component.setBorder(new EmptyBorder(null, UIManager.get("TabbedPane.contentMargin") as Insets));
            component.setLayout(new BorderLayout());
            component.appendAll(AsWingUtils.createPaneToHold(thumbnail, new CenterLayout(), "Center"), footer);
        }

        public function getCellValue(): * {
            return theme;
        }

        public function getCellComponent(): Component {
            return component;
        }
    }
}

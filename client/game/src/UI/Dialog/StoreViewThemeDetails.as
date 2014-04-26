package src.UI.Dialog {
    import com.codecatalyst.promise.Deferred;
    import com.codecatalyst.promise.Promise;

    import org.aswing.AsWingConstants;
    import org.aswing.AssetIcon;
    import org.aswing.AssetPane;
    import org.aswing.CenterLayout;
    import org.aswing.FlowLayout;
    import org.aswing.JButton;
    import org.aswing.JFrame;
    import org.aswing.JLabel;
    import org.aswing.JPanel;
    import org.aswing.JTabbedPane;
    import org.aswing.SoftBoxLayout;
    import org.aswing.VectorListModel;
    import org.aswing.ext.GeneralGridListCellFactory;
    import org.aswing.ext.GridList;
    import org.aswing.ext.GridListItemEvent;
    import org.aswing.ext.MultilineLabel;

    import src.Assets;
    import src.Global;
    import src.Objects.Factories.StructureFactory;
    import src.Objects.Prototypes.StructurePrototype;
    import src.Objects.Store.IStoreItem;
    import src.Objects.Store.StructureStoreItem;
    import src.Objects.Theme;
    import src.UI.Components.Store.StoreItemGridCell;
    import src.UI.GameJPanel;
    import src.UI.LookAndFeel.GameLookAndFeel;
    import src.UI.Tooltips.StoreItemTooltip;
    import src.Util.StringHelper;
    import src.Util.Util;

    public class StoreViewThemeDetails extends GameJPanel {
        private var theme: Theme;
        private var lblDescription: MultilineLabel;
        private var itemTooltip: StoreItemTooltip;
        private var buyDeferred: Deferred;

        public function StoreViewThemeDetails(theme: Theme) {
            this.theme = theme;
            this.buyDeferred = new Deferred();
            this.title = theme.localizedName;

            createUI();
        }

        public function getBuyItemPromise(): Promise {
            return buyDeferred.promise;
        }

        private function createUI(): void {
            setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 10));
            setPreferredWidth(540);

            var lblTitle: JLabel = new JLabel(theme.localizedName);
            GameLookAndFeel.changeClass(lblTitle, "darkHeader");

            lblDescription = new MultilineLabel(theme.localizedDescription, 0, 100);

            var pnlPreviewImage: JPanel = new JPanel(new CenterLayout());
            pnlPreviewImage.appendAll(new AssetPane(Assets.getInstance(theme.id + "_THEME_BANNER")));

            var gridStoreItems: GridList = new GridList(new VectorListModel(getThemeItems()), new GeneralGridListCellFactory(StoreItemGridCell), 5, 0);
            gridStoreItems.setTracksHeight(true);
            gridStoreItems.setTileWidth(85);
            gridStoreItems.setTileHeight(80);

            var previewTabs: JTabbedPane = new JTabbedPane();
            previewTabs.setPreferredHeight(220);
            previewTabs.appendTab(pnlPreviewImage, StringHelper.localize("STORE_VIEW_THEME_DIALOG_PREVIEW_TAB"));
            previewTabs.appendTab(Util.createTopAlignedScrollPane(gridStoreItems), StringHelper.localize("STORE_VIEW_THEME_DIALOG_DETAIL_TAB"));

            var btnBuy: JButton = new JButton(StringHelper.localize("STORE_VIEW_THEME_DIALOG_BUY", theme.cost), new AssetIcon(Assets.getInstance("ICON_COIN")));
            btnBuy.setHorizontalTextPosition(AsWingConstants.LEFT);
            btnBuy.setIconTextGap(0);

            var pnlBuyRow: JPanel = new JPanel(new FlowLayout(AsWingConstants.RIGHT, 0, 0, false));
            pnlBuyRow.appendAll(btnBuy);

            appendAll(lblTitle, lblDescription, previewTabs, pnlBuyRow);

            lblDescription.pack();

            gridStoreItems.addEventListener(GridListItemEvent.ITEM_ROLL_OUT, onItemRollOut);
            gridStoreItems.addEventListener(GridListItemEvent.ITEM_ROLL_OVER, onItemRollOver);
        }

        public function onItemRollOver(event: GridListItemEvent):void
        {
            var storeItem: IStoreItem = IStoreItem(event.getValue());

            onItemRollOut(event);
            this.itemTooltip = new StoreItemTooltip(storeItem);
            this.itemTooltip.show(this);
        }

        public function onItemRollOut(event: GridListItemEvent):void
        {
            if (this.itemTooltip) {
                this.itemTooltip.hide();
            }

            this.itemTooltip = null;
        }

        private function getThemeItems(): Array {
            var themeItems: Array = [];

            for each (var structurePrototype: StructurePrototype in StructureFactory.getAllStructureTypes()) {
                if (Assets.doesSpriteExist(structurePrototype.getSpriteName(theme.id))) {
                    themeItems.push(new StructureStoreItem(theme, structurePrototype));
                }
            }

            themeItems.sortOn("title", Array.CASEINSENSITIVE);
            return themeItems;
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

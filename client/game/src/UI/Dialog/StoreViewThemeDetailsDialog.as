package src.UI.Dialog {
    import com.codecatalyst.promise.Deferred;

    import org.aswing.*;
    import org.aswing.ext.GeneralGridListCellFactory;
    import org.aswing.ext.GridList;
    import org.aswing.ext.GridListItemEvent;
    import org.aswing.ext.MultilineLabel;

    import src.Assets;
    import src.Global;
    import src.Objects.Store.IStoreAsset;
    import src.UI.Components.Store.StoreItemAssetGridCell;
    import src.UI.GameJPanel;
    import src.UI.LookAndFeel.GameLookAndFeel;
    import src.UI.Tooltips.StoreItemTooltip;
    import src.UI.ViewModels.StoreViewThemeDetailsVM;
    import src.Util.StringHelper;
    import src.Util.Util;

    public class StoreViewThemeDetailsDialog extends GameJPanel {
        private var lblDescription: MultilineLabel;

        private var btnBuy: JButton;

        private var itemTooltip: StoreItemTooltip;

        private var buyDeferred: Deferred;

        private var viewModel: StoreViewThemeDetailsVM;

        public function StoreViewThemeDetailsDialog(viewModel: StoreViewThemeDetailsVM) {
            this.viewModel = viewModel;
            this.buyDeferred = new Deferred();
            this.title = viewModel.theme.localizedName;

            createUI();

            btnBuy.addActionListener(function(): void {
                getFrame().dispose();

                viewModel.buy();
            });
        }

        private function createUI(): void {
            setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 10));
            setPreferredWidth(540);

            var lblTitle: JLabel = new JLabel(viewModel.theme.localizedName);
            GameLookAndFeel.changeClass(lblTitle, "darkHeader");

            lblDescription = new MultilineLabel(viewModel.theme.localizedDescription, 0, 100);

            var pnlPreviewImage: JPanel = new JPanel(new CenterLayout());
            pnlPreviewImage.appendAll(new AssetPane(Assets.getInstance(viewModel.theme.id + "_THEME_BANNER")));

            var gridStoreItems: GridList = new GridList(new VectorListModel(viewModel.getThemeAssets()), new GeneralGridListCellFactory(StoreItemAssetGridCell), 5, 0);
            gridStoreItems.setTracksHeight(true);
            gridStoreItems.setTileWidth(85);
            gridStoreItems.setTileHeight(80);

            var previewTabs: JTabbedPane = new JTabbedPane();
            previewTabs.setPreferredHeight(220);
            previewTabs.appendTab(pnlPreviewImage, StringHelper.localize("STORE_VIEW_THEME_DIALOG_PREVIEW_TAB"));
            previewTabs.appendTab(Util.createTopAlignedScrollPane(gridStoreItems), StringHelper.localize("STORE_VIEW_THEME_DIALOG_DETAIL_TAB"));

            btnBuy = new JButton(StringHelper.localize("STORE_VIEW_THEME_DIALOG_BUY", viewModel.theme.cost), new AssetIcon(Assets.getInstance("ICON_COIN")));
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
            var storeItem: IStoreAsset = IStoreAsset(event.getValue());

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

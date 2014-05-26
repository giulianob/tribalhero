package src.UI.Dialog {

    import System.Linq.Enumerable;

    import flash.events.Event;

    import org.aswing.AsWingConstants;
    import org.aswing.AssetIcon;
    import org.aswing.BorderLayout;
    import org.aswing.FlowLayout;
    import org.aswing.JFrame;
    import org.aswing.JLabel;
    import org.aswing.JLabelButton;
    import org.aswing.JPanel;
    import org.aswing.SoftBoxLayout;
    import org.aswing.VectorListModel;
    import org.aswing.ext.GeneralGridListCellFactory;
    import org.aswing.ext.GridList;
    import org.aswing.ext.GridListItemEvent;
    import org.aswing.geom.IntDimension;

    import src.Assets;

    import src.Constants;
    import src.Global;
    import src.Objects.Store.StoreItem;
    import src.SessionVariables;
    import src.UI.Components.CoinLabel;
    import src.UI.Components.Store.StoreItemGridCell;
    import src.UI.GameJImagePanelBackground;
    import src.UI.GameJPanel;
    import src.UI.LookAndFeel.GameLookAndFeel;
    import src.UI.ViewModels.StoreDialogVM;
    import src.Util.StringHelper;
    import src.Util.Util;

    public class StoreDialog extends GameJPanel {

        private var allItems: Array;
        private var availableStoreItemsGridList: GridList;
        private var purchasedStoreItemsGridList: GridList;
        private var currentBalance: JLabelButton;
        
        private var viewModel: StoreDialogVM;

        public function StoreDialog(viewModel: StoreDialogVM) {
            this.viewModel = viewModel;

            title = StringHelper.localize("STORE_DIALOG_TITLE");

            createUI();

            Constants.session.addEventListener(SessionVariables.THEME_PURCHASED, function(e: Event): void {
                sortItems();
            }, false, 0, true);

            Constants.session.addEventListener(SessionVariables.COINS_UPDATE, function (e: Event): void {
                currentBalance.setText(Constants.session.coins.toString());
                currentBalance.pack();
            }, false, 0, true);

            currentBalance.addActionListener(function(e: Event): void {
               viewModel.buyCoins();
            });
        }

        private function createUI(): void {
            setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS));
            setPreferredSize(new IntDimension(700, Math.max(600, Constants.screenH - GameJImagePanelBackground.getFrameHeight())));

            var balanceLabel:JLabel = new JLabel(t("STORE_DIALOG_CURRENT_BALANCE"), null);

            currentBalance = new JLabelButton(Constants.session.coins.toString(), new AssetIcon(Assets.getInstance("ICON_COIN")), AsWingConstants.RIGHT);
            currentBalance.setHorizontalTextPosition(JLabel.LEFT);
            currentBalance.setIconTextGap(0);

            var headerPnl: JPanel = new JPanel(new BorderLayout(5));

            var lblHeader: JLabel = new JLabel(t("STORE_DIALOG_TITLE"), null, AsWingConstants.LEFT);
            lblHeader.setConstraints("Center");
            GameLookAndFeel.changeClass(lblHeader, "darkSectionHeader");

            var balanceWrapper: JPanel = new JPanel(new FlowLayout(AsWingConstants.RIGHT, 5, 0, false));
            balanceWrapper.setConstraints("East");
            balanceWrapper.appendAll(balanceLabel, currentBalance);

            headerPnl.appendAll(lblHeader, balanceWrapper);

            availableStoreItemsGridList = new GridList(new VectorListModel(), new GeneralGridListCellFactory(StoreItemGridCell), 3, 0);
            availableStoreItemsGridList.setTileWidth(200);
            availableStoreItemsGridList.setTileHeight(125);
            availableStoreItemsGridList.setTracksWidth(true);
            availableStoreItemsGridList.setPreferredWidth(400);
            availableStoreItemsGridList.addEventListener(GridListItemEvent.ITEM_CLICK, viewItemDetails);

            var itemsPurchased: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS));
            var lblPurchasedItemsHeader: JLabel = new JLabel(t("STORE_DIALOG_PURCHASED_ITEMS"), null, AsWingConstants.LEFT);
            GameLookAndFeel.changeClass(lblPurchasedItemsHeader, "darkSectionHeader");
            purchasedStoreItemsGridList = new GridList(new VectorListModel(), new GeneralGridListCellFactory(StoreItemGridCell), 3, 0);
            purchasedStoreItemsGridList.setTileWidth(200);
            purchasedStoreItemsGridList.setTileHeight(125);
            purchasedStoreItemsGridList.setTracksWidth(true);
            purchasedStoreItemsGridList.setPreferredWidth(400);
            purchasedStoreItemsGridList.addEventListener(GridListItemEvent.ITEM_CLICK, viewItemDetails);

            itemsPurchased.appendAll(lblPurchasedItemsHeader, purchasedStoreItemsGridList);

            var scrollBody: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 15));
            scrollBody.appendAll(availableStoreItemsGridList, itemsPurchased);

            appendAll(headerPnl, Util.createTopAlignedScrollPane(scrollBody));
        }

        public function viewItemDetails(e: GridListItemEvent): void {
            viewModel.viewItemDetails(StoreItem(e.getValue()));
        }

        public function show(owner:* = null, modal:Boolean = true, onClose:Function = null):JFrame
        {
            super.showSelf(owner, modal, onClose, null);

            frame.setResizable(false);
            frame.pack();

            Global.gameContainer.showFrame(frame);

            viewModel.loadItems().then(function(items: Array): void
            {
                allItems = items;
                sortItems();
            });

            return frame;
        }

        private function sortItems(): void {
            VectorListModel(availableStoreItemsGridList.getModel()).clear();
            VectorListModel(purchasedStoreItemsGridList.getModel()).clear();

            var sortedItems: Array = Enumerable.from(allItems)
                    .orderBy(function(item: StoreItem): * {
                        return item.itemType;
                    })
                    .thenBy(function(item: StoreItem): * {
                        return item.created;
                    })
                    .thenByDescending(function(item: StoreItem): * {
                        return item.cost;
                    }).toArray();

            for each (var item: StoreItem in sortedItems) {
                if (!item.hasPurchased()) {
                    VectorListModel(availableStoreItemsGridList.getModel()).append(item);
                }
                else {
                    VectorListModel(purchasedStoreItemsGridList.getModel()).append(item);
                }
            }

            getFrame().pack();
        }
    }
}
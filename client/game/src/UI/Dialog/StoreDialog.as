package src.UI.Dialog {

    import org.aswing.JFrame;
    import org.aswing.JPanel;
    import org.aswing.SoftBoxLayout;
    import org.aswing.VectorListModel;
    import org.aswing.ext.GeneralGridListCellFactory;
    import org.aswing.ext.GridList;
    import org.aswing.ext.GridListItemEvent;
    import org.aswing.geom.IntDimension;

    import src.Constants;
    import src.Global;
    import src.Objects.Store.StoreItem;
    import src.Objects.Theme;
    import src.UI.Components.Store.StoreItemGridCell;
    import src.UI.GameJImagePanelBackground;
    import src.UI.GameJPanel;
    import src.UI.ViewModels.StoreDialogVM;
    import src.Util.StringHelper;
    import src.Util.Util;

    public class StoreDialog extends GameJPanel {

        private var availableStoreItemsGridList: GridList;
        private var viewModel: StoreDialogVM;

        public function StoreDialog(viewModel: StoreDialogVM) {
            this.viewModel = viewModel;

            title = StringHelper.localize("STORE_DIALOG_TITLE");

            createUI();
        }

        private function createUI(): void {
            setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS));
            setPreferredSize(new IntDimension(600, Math.max(600, Constants.screenH - GameJImagePanelBackground.getFrameHeight())));

            availableStoreItemsGridList = new GridList(new VectorListModel(), new GeneralGridListCellFactory(StoreItemGridCell), 3, 0);
            availableStoreItemsGridList.setTileWidth(175);
            availableStoreItemsGridList.setTileHeight(125);
            availableStoreItemsGridList.setTracksWidth(true);
            availableStoreItemsGridList.setPreferredSize(new IntDimension(400, 400));

            availableStoreItemsGridList.addEventListener(GridListItemEvent.ITEM_CLICK, viewItemDetails);

            var scrollBody: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS));
            scrollBody.append(availableStoreItemsGridList);

            appendAll(Util.createTopAlignedScrollPane(scrollBody));
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

            viewModel.loadItems().then(function(items: Array): void {
                VectorListModel(availableStoreItemsGridList.getModel()).appendAll(items);
            });

            return frame;
        }
    }
}
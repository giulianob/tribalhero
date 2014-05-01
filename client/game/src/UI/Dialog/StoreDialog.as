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
    import src.Objects.Theme;
    import src.UI.Components.Store.StoreThemeGridCell;
    import src.UI.GameJImagePanelBackground;
    import src.UI.GameJPanel;
    import src.UI.ViewModels.StoreDialogVM;
    import src.Util.StringHelper;
    import src.Util.Util;

    public class StoreDialog extends GameJPanel {

        private var availableThemesGridList: GridList;
        private var viewModel: StoreDialogVM;

        public function StoreDialog(viewModel: StoreDialogVM) {
            this.viewModel = viewModel;

            title = StringHelper.localize("STORE_DIALOG_TITLE");

            createUI();

            VectorListModel(availableThemesGridList.getModel()).appendAll(viewModel.themes);
        }

        private function createUI(): void {
            setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS));
            setPreferredSize(new IntDimension(600, Math.max(600, Constants.screenH - GameJImagePanelBackground.getFrameHeight())));

            availableThemesGridList = new GridList(new VectorListModel(), new GeneralGridListCellFactory(StoreThemeGridCell), 3, 0);
            availableThemesGridList.setTileWidth(175);
            availableThemesGridList.setTileHeight(125);
            availableThemesGridList.setTracksWidth(true);
            availableThemesGridList.setPreferredSize(new IntDimension(400, 400));

            availableThemesGridList.addEventListener(GridListItemEvent.ITEM_CLICK, viewThemeDetails);

            var scrollBody: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS));
            scrollBody.append(availableThemesGridList);

            appendAll(Util.createTopAlignedScrollPane(scrollBody));
        }

        public function viewThemeDetails(e: GridListItemEvent): void {
            viewModel.viewThemeDetails(Theme(e.getValue()));
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
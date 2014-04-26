package src.UI.Components.Store {
    import flash.display.Sprite;

    import org.aswing.AsWingConstants;
    import org.aswing.AssetPane;
    import org.aswing.Component;
    import org.aswing.ext.GridList;
    import org.aswing.ext.GridListCell;

    import src.Objects.Store.IStoreItem;
    import src.Util.Util;

    public class StoreItemGridCell implements GridListCell {
        private var storeItem: IStoreItem;
        private var icon: AssetPane;

        public function StoreItemGridCell() {
            this.icon = new AssetPane();
        }

        public function setGridListCellStatus(gridList: GridList, selected: Boolean, index: int): void {
        }

        public function setCellValue(value: *): void {
            this.storeItem = IStoreItem(value);

            var thumbnail: Sprite = storeItem.thumbnail();
            Util.resizeSprite(thumbnail, 75, 75);
            icon.setHorizontalAlignment(AsWingConstants.CENTER);
            icon.setVerticalAlignment(AsWingConstants.CENTER);
            icon.setAsset(thumbnail);
        }

        public function getCellValue(): * {
            return storeItem;
        }

        public function getCellComponent(): Component {
            return icon;
        }
    }
}

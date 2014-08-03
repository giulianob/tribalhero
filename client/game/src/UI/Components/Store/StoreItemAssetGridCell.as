package src.UI.Components.Store {
    import flash.display.Sprite;

    import org.aswing.AsWingConstants;
    import org.aswing.AssetPane;
    import org.aswing.Component;
    import org.aswing.ext.GridList;
    import org.aswing.ext.GridListCell;

    import src.Objects.Store.IStoreAsset;
    import src.Util.Util;

    public class StoreItemAssetGridCell implements GridListCell {
        private var storeItem: IStoreAsset;
        private var icon: AssetPane;

        public function StoreItemAssetGridCell() {
            this.icon = new AssetPane();
        }

        public function setGridListCellStatus(gridList: GridList, selected: Boolean, index: int): void {
        }

        public function setCellValue(value: *): void {
            this.storeItem = IStoreAsset(value);

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

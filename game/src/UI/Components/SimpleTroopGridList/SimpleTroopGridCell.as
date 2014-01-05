package src.UI.Components.SimpleTroopGridList
{
    import org.aswing.*;
    import org.aswing.border.*;
    import org.aswing.dnd.DragManager;
    import org.aswing.dnd.SourceData;
    import org.aswing.event.DragAndDropEvent;
    import org.aswing.ext.*;
    import org.aswing.geom.*;

    import src.Objects.Troop.Unit;

    public class SimpleTroopGridCell extends JLabel implements GridListCell{

		protected var value: *;

		public function SimpleTroopGridCell(){
			super();

			addEventListener(DragAndDropEvent.DRAG_RECOGNIZED, __dragRecognized);
			
			setBorder(new LineBorder(null, ASColor.BLACK, 1, 3));
			setPreferredSize(new IntDimension(45, 30));
			setHorizontalTextPosition(AsWingConstants.LEFT);
			setIconTextGap(0);
		}

		private function __dragRecognized(e: DragAndDropEvent) : void{
			DragManager.startDrag(this, new SourceData("dragData", {"data": value.data}), new SimpleTroopDraggingImage(e.getDragInitiator()));
		}

		public function setCellValue(value:*):void{
			this.value = value;

			setIcon(new AssetIcon(value.source));
			var unit: Unit = value.data;
			setText(unit.count.toString());
			repaintAndRevalidate();
		}

		public function getCellValue():*{
			return value;
		}

		public function getCellComponent():Component{
			return this;
		}

		public function setGridListCellStatus(gridList:GridList, selected:Boolean, index:int):void {
		}

	}

}


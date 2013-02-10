package src.UI.Components.TroopStubGridList
{
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;

	public class TroopStubGridCell extends JLabel implements GridListCell {

		protected var value: *;

		public function TroopStubGridCell(){
			super();
			
			buttonMode = true;
			setPreferredSize(new IntDimension(20, 20));
			setOpaque(false);
		}

		public function setCellValue(value:*):void{
			this.value = value;

			setIcon(new AssetIcon(value.source));
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

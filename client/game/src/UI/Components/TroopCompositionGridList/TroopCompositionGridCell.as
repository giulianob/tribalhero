package src.UI.Components.TroopCompositionGridList
{
    import org.aswing.*;
    import org.aswing.ext.*;
    import org.aswing.geom.*;

    import src.Objects.Troop.Unit;

    public class TroopCompositionGridCell extends JLabel implements GridListCell{

		protected var value: *;

		public function TroopCompositionGridCell(){
			super();

			setPreferredSize(new IntDimension(45, 20));			
			setHorizontalTextPosition(AsWingConstants.LEFT);
			setVerticalAlignment(AsWingConstants.TOP);
			setIconTextGap(0);			
		}

		public function setCellValue(value:*):void{
			this.value = value;

			setIcon(new AssetIcon(value.source));
			
			var unit: Unit = value.data;
			setText(unit.count.toString());
			
			/*if (value.tooltipMode) 
				GameLookAndFeel.changeClass(this, "Tooltip.text Label.small");		*/
				
			pack();
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

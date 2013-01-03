package src.UI.Components.ComplexTroopGridList
{
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import src.Objects.Troop.Unit;
	import src.UI.LookAndFeel.GameLookAndFeel;

	public class ComplexTroopGridCell extends JLabel implements GridListCell{

		protected var value: *;

		public function ComplexTroopGridCell(){
			super();

			setPreferredSize(new IntDimension(45, 20));			
			setHorizontalTextPosition(AsWingConstants.LEFT);
			setIconTextGap(0);			
		}

		public function setCellValue(value:*):void{
			this.value = value;

			setIcon(new AssetIcon(value.source));
			
			var unit: Unit = value.data;
			setText(unit.count.toString());
			
			if (value.tooltipMode) 
				GameLookAndFeel.changeClass(this, "Tooltip.text Label.small");		
				
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

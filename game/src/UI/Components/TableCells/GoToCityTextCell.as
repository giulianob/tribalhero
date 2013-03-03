package src.UI.Components.TableCells
{
	import flash.events.*;
	import org.aswing.*;
	import org.aswing.event.*;
	import org.aswing.table.*;
	import src.*;
	import src.UI.Components.*;

	public class GoToCityTextCell extends AbstractTableCell
	{
		protected var icon: GoToCityIcon;

		// We have to create a wrapper because the cells ui will be forced to the entire area of the container
		protected var wrapper: JPanel;

		public function GoToCityTextCell()
		{
			super();
			
			wrapper = new JPanel(new CenterLayout());
			wrapper.setOpaque(true);
			
			setCellValue(value);
		}

		override public function setCellValue(value:*):void
		{
			super.setCellValue(value);
			if (icon) {
				icon.getAsset().removeEventListener(AWEvent.ACT, onGoToCity);
				wrapper.removeAll();
				icon = null;	
			}						
			
			icon = new GoToCityIcon(value);
			icon.getAsset().addEventListener(AWEvent.ACT, onGoToCity);
			
			wrapper.append(new AssetPane(icon.getAsset()));
		}
		
		private function onGoToCity(e: Event): void 
		{
			Global.gameContainer.clearAllSelections();
			Global.gameContainer.closeAllFrames(true);
		}

		override public function getCellValue():*
		{
			return value;
		}

		override public function getCellComponent():Component
		{
			return wrapper;
		}
		
		public function goToCity() : void {
			if (icon) 
				icon.goToCity();
		}

		override public function setTableCellStatus(table:JTable, isSelected:Boolean, row:int, column:int):void{
			if(isSelected){
				wrapper.setBackground(table.getSelectionBackground());
				wrapper.setForeground(table.getSelectionForeground());
			}else{
				wrapper.setBackground(table.getBackground());
				wrapper.setForeground(table.getForeground());
			}
		}
	}

}


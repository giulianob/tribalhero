package src.UI.Components.TableCells
{
	import flash.events.Event;
	import flash.events.MouseEvent;
	import org.aswing.AssetPane;
	import org.aswing.CenterLayout;
	import org.aswing.Component;
	import org.aswing.event.AWEvent;
	import org.aswing.JCheckBox;
	import org.aswing.JPanel;
	import org.aswing.JTable;
	import org.aswing.JToggleButton;
	import org.aswing.table.AbstractTableCell;
	import src.Global;
	import src.UI.Components.GoToCityIcon;

	/**
	 * ...
	 * @author Giuliano Barberi
	 */
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


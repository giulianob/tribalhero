package src.UI.Components.TroopsDialogTable 
{
	import org.aswing.table.AbstractTableCell;
	import org.aswing.*;
	
	public class TroopUnitsCell extends AbstractTableCell 
	{
		
		private var pnl: JPanel = new JPanel(new FlowLayout(AsWingConstants.LEFT, 0, 0, false));
		
		override public function getCellComponent():Component 
		{
			return pnl;
		}
	}

}
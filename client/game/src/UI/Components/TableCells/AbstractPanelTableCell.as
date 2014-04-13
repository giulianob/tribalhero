package src.UI.Components.TableCells 
{
    import flash.events.*;

    import org.aswing.*;
    import org.aswing.table.*;

    public class AbstractPanelTableCell extends AbstractTableCell
	{
		protected var pnl: JPanel;
		
		public function AbstractPanelTableCell() 
		{
			super();			
			
			pnl = new JPanel(getCellLayout());
			
			// Makes it so we can properly click on items such as buttons inside of this cell
			pnl.addEventListener(MouseEvent.MOUSE_DOWN, function(e: Event):void {
				e.stopImmediatePropagation();
			});			
		}
		
		override public function getCellComponent():Component 
		{
			return pnl;
		}
		
		protected function getCellPanel():JPanel {
			return pnl;
		}
		
		protected function getCellLayout(): LayoutManager {
			return new FlowLayout(AsWingConstants.LEFT, 5, 5, false);
		}
		
	}

}
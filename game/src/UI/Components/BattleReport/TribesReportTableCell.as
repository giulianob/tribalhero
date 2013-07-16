package src.UI.Components.BattleReport
{
    import flash.events.MouseEvent;

    import org.aswing.*;
    import org.aswing.table.*;

    import src.UI.Components.*;

    public class TribesReportTableCell extends AbstractTableCell
	{		
		private var pnl: JPanel = new JPanel(new FlowLayout(AsWingConstants.LEFT, 10, 0, false));
		
		public function TribesReportTableCell() 
		{
			super();
			
			pnl.setOpaque(true);											
		}
		
		override public function setCellValue(value:*): void
		{			
			super.setCellValue(value);
			
			pnl.removeAll();			

			for each (var tribe: * in value) {
				var tribeLabel: TribeLabel = new TribeLabel(tribe.id, tribe.name, false);
				tribeLabel.addEventListener(MouseEvent.MOUSE_DOWN, function (e: MouseEvent): void {
					e.stopImmediatePropagation();
				}, false, 0, true);
			
				pnl.append(tribeLabel);
			}
		}
		
		override public function getCellComponent():Component 
		{			
			return pnl;
		}
	}
}


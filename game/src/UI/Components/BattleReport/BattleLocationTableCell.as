package src.UI.Components.BattleReport
{
	import src.Util.StringHelper;
	import flash.events.MouseEvent;
	import org.aswing.*;
	import org.aswing.table.*;
	import src.Objects.Battle.BattleLocation;
	import src.UI.Components.*;

	public class BattleLocationTableCell extends AbstractTableCell
	{		
		private var pnl: JPanel = new JPanel(new FlowLayout(AsWingConstants.LEFT, 0, 0, false));
		
		public function BattleLocationTableCell() 
		{
			super();					
			pnl.setOpaque(true);
		}
		
		override public function setCellValue(value:*): void
		{			
			super.setCellValue(value);
			
			pnl.removeAll();
			
			var locationLabel: BattleLocationLabel = new BattleLocationLabel(new BattleLocation(value.type, value.id, value.name));
			locationLabel.addEventListener(MouseEvent.MOUSE_DOWN, function (e: MouseEvent): void {
				e.stopImmediatePropagation();
			}, false, 0, true);
				
			pnl.append(locationLabel);
		}
		
		override public function getCellComponent():Component 
		{			
			return pnl;
		}
	}
}


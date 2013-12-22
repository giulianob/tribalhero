package src.UI.Components.BattleReport
{
    import org.aswing.AsWingConstants;
    import org.aswing.Component;
    import org.aswing.FlowLayout;
    import org.aswing.JLabelButton;
    import org.aswing.JPanel;
    import org.aswing.JTable;
    import org.aswing.table.AbstractTableCell;

    import src.UI.Components.SimpleTooltip;
    import src.UI.LookAndFeel.GameLookAndFeel;
    import src.Util.StringHelper;

    public class UnreadTextCell extends AbstractTableCell
	{			
		protected var lbl: JLabelButton = new JLabelButton("", null, AsWingConstants.LEFT);
		private var pnl: JPanel = new JPanel(new FlowLayout(AsWingConstants.LEFT, 0, 0, false));
		
		public function UnreadTextCell() 
		{
			super();
			
			pnl.append(lbl);
			
			pnl.setOpaque(true);
			
			new SimpleTooltip(lbl, StringHelper.localize("STR_VIEW_REPORT"));
		}
		
		override public function setCellValue(value:*):void
		{
			this.value = value;
			
			if (value.unread) {
				lbl.setFont(GameLookAndFeel.getClassAttribute("Message.unread", "LabelButton.font"));
			} else {
				lbl.setFont(GameLookAndFeel.getClassAttribute("Message.read", "LabelButton.font"));
			}

			lbl.setText(value[getCellProperty()]);
		}

		override public function setTableCellStatus(table:JTable, isSelected:Boolean, row:int, column:int):void
		{
			if(isSelected){
				pnl.setBackground(table.getSelectionBackground());
				pnl.setForeground(table.getSelectionForeground());
			}else{
				pnl.setBackground(table.getBackground());
				pnl.setForeground(table.getForeground());
			}
		}
		
		protected function getCellProperty(): String {
			throw new Error("Should be overriden");
		}
		
		override public function getCellComponent():Component
		{			
			return pnl;
		}		
	}

}


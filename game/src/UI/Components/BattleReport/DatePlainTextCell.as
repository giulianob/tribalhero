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
    import src.Util.DateUtil;
    import src.Util.StringHelper;

    public class DatePlainTextCell extends AbstractTableCell
	{			
		private var lbl: JLabelButton = new JLabelButton("", null, AsWingConstants.LEFT);
		private var pnl: JPanel = new JPanel(new FlowLayout(AsWingConstants.LEFT, 0, 0, false));
		
		public function DatePlainTextCell() 
		{
			super();
			
			pnl.appendAll(lbl);
			pnl.setOpaque(true);
			
			new SimpleTooltip(lbl, StringHelper.localize("STR_VIEW_REPORT"));
		}
		
		override public function setCellValue(value:*):void
		{
			this.value = value;
			
			lbl.setFont(GameLookAndFeel.getClassAttribute("Message.read", "LabelButton.font"));

			lbl.setText(DateUtil.niceShort(value));
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
		
		override public function getCellComponent():Component
		{			
			return pnl;
		}		
	}

}


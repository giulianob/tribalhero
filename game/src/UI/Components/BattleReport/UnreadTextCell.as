package src.UI.Components.BattleReport
{
	import org.aswing.JTable;
	import org.aswing.table.DefaultTextCell;
	import src.UI.LookAndFeel.GameLookAndFeel;

	public class UnreadTextCell extends DefaultTextCell
	{			
		override public function setCellValue(value:*):void
		{
			this.value = value;

			if (value.unread) {
				setFont(GameLookAndFeel.getClassAttribute("Message.unread", "Label.font"));
				setForeground(GameLookAndFeel.getClassAttribute("Message.unread", "Label.foreground"));
			} else {
				setFont(GameLookAndFeel.getClassAttribute("Message.read", "Label.font"));
				setForeground(GameLookAndFeel.getClassAttribute("Message.read", "Label.foreground"));
			}

			setText(value[getCellProperty()]);
		}

		override public function setTableCellStatus(table:JTable, isSelected:Boolean, row:int, column:int):void
		{
			if(isSelected){
				setBackground(table.getSelectionBackground());
				setForeground(table.getSelectionForeground());
			}else{
				setBackground(table.getBackground());
				setForeground(table.getForeground());
			}
		}
		
		protected function getCellProperty(): String {
			throw new Error("Should be overriden");
		}
	}

}


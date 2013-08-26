package src.UI.Components.TableCells
{
    import org.aswing.*;
    import org.aswing.table.*;

    import src.*;
    import src.UI.LookAndFeel.GameLookAndFeel;
    import src.Util.DateUtil;
    import src.Util.StringHelper;

    public class MessageBoardThreadListCell extends AbstractTableCell
	{
		protected var wrapper: JPanel;
		
		private var lblSubject: JLabel;
        
        private var lblLastPost: JLabel;
        ;
		
		public function MessageBoardThreadListCell()
		{
			super();
			
			wrapper = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 0));
			wrapper.setOpaque(true);

            lblSubject = new JLabel("", null, AsWingConstants.LEFT);
            lblLastPost = new JLabel("", null, AsWingConstants.LEFT);            
		}
		
		override public function setTableCellStatus(table:JTable, isSelected:Boolean, row:int, column:int):void 
		{
			super.setTableCellStatus(table, isSelected, row, column);
			
			if (isSelected && getCellValue()) {
                getCellValue().lastReadTimestamp = Global.map.getServerTime();
				GameLookAndFeel.changeClass(lblSubject, "Message.read");			
			}
		}

		override public function setCellValue(value:*):void
		{                                                         
            super.setCellValue(value);                        
            
            if (!value) {
                return;
            }            
            
            lblSubject.setText(StringHelper.truncate(value.subject, 100));
            lblLastPost.setText("Last post " + DateUtil.getRelativeFromTimestamp(value.lastPostTimestamp) + " by " + value.lastPostPlayerName);
            
			wrapper.removeAll();				
			
			// Change to unread if it has never been read or there have not been any new messages since last time we read it
			if ((!value.lastReadTimestamp || value.lastReadTimestamp < value.lastPostTimestamp)) {
				GameLookAndFeel.changeClass(lblSubject, "Message.unread");
			} else {
				GameLookAndFeel.changeClass(lblSubject, "Message.read");
			}			
			
			wrapper.appendAll(lblSubject, lblLastPost);
		}
		
		override public function getCellValue(): *
		{
			return value;
		}

		override public function getCellComponent():Component
		{
			return wrapper;
		}
	}

}


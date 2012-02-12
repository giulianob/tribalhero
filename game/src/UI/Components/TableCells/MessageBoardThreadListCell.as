package src.UI.Components.TableCells
{
	import flash.events.*;
	import org.aswing.*;
	import org.aswing.event.*;
	import org.aswing.table.*;
	import src.*;
	import src.UI.Components.*;
	import src.UI.Components.Messaging.MessagingIcon;
	import src.UI.Components.Tribe.SetRankIcon;
	import src.UI.LookAndFeel.GameLookAndFeel;
	import src.Util.StringHelper;
	import src.Util.Util;

	public class MessageBoardThreadListCell extends AbstractTableCell
	{
		protected var wrapper: JPanel;
		
		private var lblSubject: JLabel;
		
		public function MessageBoardThreadListCell()
		{
			super();
			
			wrapper = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 0));
			wrapper.setOpaque(true);
		}
		
		override public function setTableCellStatus(table:JTable, isSelected:Boolean, row:int, column:int):void 
		{
			super.setTableCellStatus(table, isSelected, row, column);
			
			if (isSelected) {
				GameLookAndFeel.changeClass(lblSubject, "Message.read");			
			}
		}

		override public function setCellValue(value:*):void
		{
			if (this.getCellValue() && this.getCellValue().id == value.id) {
				return;
			}
			
			super.setCellValue(value);
			wrapper.removeAll();
			
			lblSubject = new JLabel(StringHelper.truncate(value.subject, 100), null, AsWingConstants.LEFT);
			
			// Change to unread if it has never been read or there have not been any new messages since last time we read it
			if ((!value.lastReadTimestamp || value.lastReadTimestamp < value.lastPostTimestamp)) {
				GameLookAndFeel.changeClass(lblSubject, "Message.unread");
			} else {
				GameLookAndFeel.changeClass(lblSubject, "Message.read");
			}

			var lblLastPost: JLabel = new JLabel("Last post " + value.lastPostAgoInWords + " by " + value.lastPostPlayerName, null, AsWingConstants.LEFT);
			
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


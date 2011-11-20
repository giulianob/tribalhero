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
		
		public function MessageBoardThreadListCell()
		{
			super();
			
			wrapper = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 0));
			wrapper.setOpaque(true);
		}

		override public function setCellValue(value:*):void
		{
			super.setCellValue(value);
			wrapper.removeAll();
			
			var lblSubject: JLabel = new JLabel(StringHelper.truncate(value.subject, 100), null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblSubject, "darkText");
			
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


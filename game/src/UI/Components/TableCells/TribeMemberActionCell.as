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

	public class TribeMemberActionCell extends AbstractTableCell
	{
		protected var btnSendMessage: MessagingIcon;
		protected var btnSetRank: SetRankIcon;
		
		protected var wrapper: JPanel;

		public function TribeMemberActionCell()
		{
			super();
			
			wrapper = new JPanel(new FlowLayout(AsWingConstants.RIGHT, 5, 0, true));
			wrapper.setOpaque(true);
		}

		override public function setCellValue(value:*):void
		{
			super.setCellValue(value);
			wrapper.removeAll();
			
			// Show messaging icon if it's not yourself
			if (Constants.playerId != value.playerId) {
				btnSendMessage = new MessagingIcon(value.playerName);
				wrapper.append(new AssetPane(btnSendMessage.getAsset()));
			}
			
			// Only show set rank if rank is chief/elder and other guy isn't the chief
			if (Constants.tribeRank <= 1 && value.rank > 0) {
				btnSetRank = new SetRankIcon(value.playerId, value.rank);
				wrapper.append(new AssetPane(btnSetRank.getAsset()));
			}
		}

		override public function getCellValue():*
		{
			return value;
		}

		override public function getCellComponent():Component
		{
			return wrapper;
		}
	}

}


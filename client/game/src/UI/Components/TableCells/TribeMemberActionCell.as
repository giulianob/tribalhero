package src.UI.Components.TableCells
{
    import org.aswing.*;
    import org.aswing.table.*;

    import src.*;
    import src.Objects.Tribe;
    import src.UI.Components.Messaging.MessagingIcon;
    import src.UI.Components.Tribe.KickIcon;
    import src.UI.Components.Tribe.SetRankIcon;

    public class TribeMemberActionCell extends AbstractTableCell
	{
		protected var btnSendMessage: MessagingIcon;
		protected var btnSetRank: SetRankIcon;
		protected var btnKick: KickIcon;
		
		protected var wrapper: JPanel;
		private var profileData: * 
		;
		public function TribeMemberActionCell()
		{
			super();
			
			wrapper = new JPanel(new FlowLayout(AsWingConstants.RIGHT, 5, 0, true));
			wrapper.setOpaque(true);
			this.profileData = profileData;
		}

		override public function setCellValue(value:*):void
		{
			super.setCellValue(value);
			wrapper.removeAll();
			
			// Only show set rank if player is chief
			if (Constants.tribe.hasRight(Tribe.SET_RANK)) {
				btnSetRank = new SetRankIcon(value.playerId, value.rank);
				wrapper.append(new AssetPane(btnSetRank.getAsset()));
			}					
			
			// Show icons that aren't for yourself
			if (Constants.playerId != value.playerId) {
				if (Constants.tribe.hasRight(Tribe.KICK)) {
					btnKick = new KickIcon(value.playerId);
					wrapper.append(new AssetPane(btnKick.getAsset()));					
				}
				
				btnSendMessage = new MessagingIcon(value.playerName);
				wrapper.append(new AssetPane(btnSendMessage.getAsset()));
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


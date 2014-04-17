package src.UI.Components 
{
    import flash.events.Event;
    import flash.events.MouseEvent;

    import org.aswing.AsWingConstants;
    import org.aswing.Insets;
    import org.aswing.JLabelButton;
    import org.aswing.border.EmptyBorder;

    import src.Global;
    import src.Map.Username;
    import src.UI.Dialog.InfoDialog;
    import src.UI.Dialog.PlayerProfileDialog;

    /**
	 * ...
	 * @author Giuliano Barberi
	 */
	public class PlayerLabel extends JLabelButton
	{
		private var playerId: int = 0;
		
		private var loadingPanel: InfoDialog;
		
		public function PlayerLabel(playerId: int, playerName: String = null)
		{
			super("-");
			
			setHorizontalAlignment(AsWingConstants.LEFT);
			
			setBorder(new EmptyBorder(null, new Insets()));
			setMargin(new Insets());
			
			this.playerId = playerId;			
			
			if (playerName)
				setText(playerName);
			else
				Global.map.usernames.players.getUsername(playerId, onReceiveUsername);
			
			if (playerId > 0) {
				new SimpleTooltip(this, "View profile");
				addEventListener(MouseEvent.MOUSE_DOWN, onClick);
			}
		}
		
		private function onClick(e: Event = null) : void {
			loadingPanel = InfoDialog.showMessageDialog("TribalHero", "Loading...", null, null, true, false, 0);
			Global.mapComm.City.viewPlayerProfile(playerId, onReceiveProfile);
		}
		
		private function onReceiveProfile(profileData: * ) : void {
			if (loadingPanel)
				loadingPanel.getFrame().dispose();
				
			loadingPanel = null;
			
			if (!profileData) 
				return;
			
			var dialog: PlayerProfileDialog = new PlayerProfileDialog(profileData);
			dialog.show();
		}
		
		private function onReceiveUsername(username: Username, custom: *) : void {
			setText(username.name);
			repaintAndRevalidate();
		}
		
	}

}
package src.UI.Components 
{
	import flash.events.Event;
	import flash.events.MouseEvent;
	import org.aswing.AsWingConstants;
	import org.aswing.border.EmptyBorder;
	import org.aswing.Insets;
	import org.aswing.JLabelButton;
	import org.aswing.JPanel;
	import src.Global;
	import src.Map.Username;
	import src.UI.Dialog.InfoDialog;
	import src.UI.Dialog.TribePublicProfileDialog;
	
	/**
	 * ...
	 * @author Anthony Lam
	 */
	public class TribeLabel extends JLabelButton
	{
		private var tribeId: int = 0;
		private var tribeName: String = "Not Loaded";
		
		private var loadingPanel: InfoDialog;
		
		public function TribeLabel(tribeId: int, tribeName: String = null)
		{
			super("-");
			
			setHorizontalAlignment(AsWingConstants.LEFT);
			
			setBorder(new EmptyBorder(null, new Insets()));
			setMargin(new Insets());
			
			this.tribeId = tribeId;			
			
			if (tribeName) {
				setText(tribeName);
				this.tribeName = tribeName;
			} else
				Global.map.usernames.tribes.getUsername(tribeId, onReceiveUsername);
			
			if (tribeId > 0) {
				new SimpleTooltip(this, "View profile");
				addEventListener(MouseEvent.MOUSE_DOWN, onClick);
			}
		}
		
		private function onClick(e: Event = null) : void {
			loadingPanel = InfoDialog.showMessageDialog("TribalHero", "Loading...", null, null, true, false, 0);
			Global.mapComm.Tribe.viewTribePublicProfile({tribeId: tribeId, tribeName: tribeName}, onReceiveProfile);
		}
		
		private function onReceiveProfile(profileData: * ) : void {
			if (loadingPanel)
				loadingPanel.getFrame().dispose();
				
			loadingPanel = null;
			
			if (!profileData) 
				return;
			
			var dialog: TribePublicProfileDialog = new TribePublicProfileDialog(profileData);
			dialog.show();
		}
		
		private function onReceiveUsername(username: Username, custom: *) : void {
			setText(username.name);
			repaintAndRevalidate();
		}
		
	}

}
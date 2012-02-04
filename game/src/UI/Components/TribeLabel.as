package src.UI.Components
{
	import flash.events.Event;
	import flash.events.MouseEvent;
	import org.aswing.AsWingConstants;
	import org.aswing.border.EmptyBorder;
	import org.aswing.Insets;
	import org.aswing.JLabelButton;
	import org.aswing.JPanel;
	import src.Constants;
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
		private var tribeId:int = 0;
		private var tribeName:String = "Not Loaded";
		
		private var loadingPanel:InfoDialog;
		
		public function TribeLabel(tribeId:int, tribeName:String = null)
		{
			super("-");
			
			setHorizontalAlignment(AsWingConstants.LEFT);
			
			setBorder(new EmptyBorder(null, new Insets()));
			setMargin(new Insets());
			
			this.tribeId = tribeId;
			
			if (tribeName)
			{
				setText(tribeName);
				this.tribeName = tribeName;
			}
			else
				Global.map.usernames.tribes.getUsername(tribeId, onReceiveUsername);
			
			if (tribeId > 0)
			{
				new SimpleTooltip(this, "View profile");
				addEventListener(MouseEvent.MOUSE_DOWN, onClick);
			}
		}
		
		private function onClick(e:Event = null):void
		{
			if (Constants.tribeId == tribeId)
			{
				Global.mapComm.Tribe.viewTribeProfile();
			}
			else
			{
				Global.mapComm.Tribe.viewTribePublicProfile(tribeId);
			}
		}
		
		private function onReceiveUsername(username:Username, custom:*):void
		{
			setText(username.name);
			repaintAndRevalidate();
		}
	
	}

}
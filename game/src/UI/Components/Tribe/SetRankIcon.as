package src.UI.Components.Tribe
{
	import flash.display.MovieClip;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import org.aswing.AssetIcon;
	import src.Global;
	import src.UI.Components.SimpleTooltip;
	import src.UI.Dialog.MessageCreateDialog;
	import src.UI.Dialog.TribeSetRankDialog;

	public class SetRankIcon extends AssetIcon
	{
		private var icon: MovieClip = new ICON_SINGLE_SWORD();

		public function SetRankIcon(playerId: int, currentRank: int)
		{
			super(icon);

			icon.buttonMode = true;
			icon.mouseEnabled = true;
			new SimpleTooltip(icon, "Set Rank");
			icon.addEventListener(MouseEvent.MOUSE_DOWN, function(e: MouseEvent) : void {				
				var dialog: TribeSetRankDialog = new TribeSetRankDialog(playerId, currentRank, function(sender: TribeSetRankDialog) : void {										
					sender.getFrame().dispose();
					
					Global.mapComm.Tribe.setRank(playerId, sender.getNewRank());
				});
				
				dialog.show();
			});
		}

	}

}

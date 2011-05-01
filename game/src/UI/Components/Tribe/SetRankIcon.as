package src.UI.Components.Tribe
{
	import flash.display.MovieClip;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import org.aswing.AssetIcon;
	import src.UI.Components.SimpleTooltip;
	import src.UI.Dialog.MessageCreateDialog;

	public class SetRankIcon extends AssetIcon
	{
		private var to: String;
		private var icon: MovieClip = new ICON_SINGLE_SWORD();

		public function SetRankIcon(playerId: int, currentRank: int)
		{
			super(icon);

			icon.buttonMode = true;
			icon.mouseEnabled = true;
			new SimpleTooltip(icon, "Set Rank");
			icon.addEventListener(MouseEvent.MOUSE_DOWN, function(e: MouseEvent) : void {				

			});
		}

	}

}

﻿package src.UI.Components.Tribe
{
	import flash.display.MovieClip;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import org.aswing.AssetIcon;
	import org.aswing.JOptionPane;
	import src.Global;
	import src.UI.Components.SimpleTooltip;
	import src.UI.Dialog.InfoDialog;
	import src.UI.Dialog.MessageCreateDialog;
	import src.UI.Dialog.TribeSetRankDialog;

	public class KickIcon extends AssetIcon
	{		
		private var icon: MovieClip = new ICON_REDX();

		public function KickIcon(playerId: int)
		{
			super(icon);

			icon.buttonMode = true;
			icon.mouseEnabled = true;
			new SimpleTooltip(icon, "Kick User");
			icon.addEventListener(MouseEvent.MOUSE_DOWN, function(e: MouseEvent) : void {		
				InfoDialog.showMessageDialog("Kick tribesman", "Are you sure you want to kick this player from your tribe?", function(result: int): void {
					if (result == JOptionPane.YES)
						Global.mapComm.Tribe.kick(playerId);
						
				}, null, true, true, JOptionPane.YES | JOptionPane.NO);
			});
		}

	}

}

package src.UI.Components.Tribe
{
    import flash.display.MovieClip;
    import flash.display.Sprite;
    import flash.events.MouseEvent;

    import org.aswing.AssetIcon;

    import src.Global;
    import src.Objects.Factories.SpriteFactory;
    import src.UI.Components.SimpleTooltip;
    import src.UI.Dialog.TribeSetRankDialog;

    public class SetRankIcon extends AssetIcon
	{
		private var icon: Sprite;

		public function SetRankIcon(playerId: int, currentRank: int)
		{
            icon = SpriteFactory.getFlashSprite("ICON_SINGLE_SWORD");

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

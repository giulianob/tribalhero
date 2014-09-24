package src.UI.Components.Messaging
{
    import flash.display.Sprite;
    import flash.events.MouseEvent;

    import org.aswing.AssetIcon;

    import src.Objects.Factories.SpriteFactory;
    import src.UI.Components.SimpleTooltip;
    import src.UI.Dialog.MessageCreateDialog;

    public class MessagingIcon extends AssetIcon
	{
		private var icon: Sprite;

		public function MessagingIcon(to: String)
		{
            icon = SpriteFactory.getFlashSprite("ICON_MESSAGE");

			super(icon);

			icon.buttonMode = true;
			icon.mouseEnabled = true;
			new SimpleTooltip(icon, "Send message");
			icon.addEventListener(MouseEvent.MOUSE_DOWN, function(e: MouseEvent) : void {		
				e.stopImmediatePropagation();
				
				var messagingDialog: MessageCreateDialog = new MessageCreateDialog(function(dialog: MessageCreateDialog) : void {
					dialog.getFrame().dispose();
				}, to);
				
				messagingDialog.show();
			});
		}

	}

}

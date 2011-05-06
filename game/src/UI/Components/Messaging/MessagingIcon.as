package src.UI.Components.Messaging
{
	import flash.display.MovieClip;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import org.aswing.AssetIcon;
	import src.UI.Components.SimpleTooltip;
	import src.UI.Dialog.MessageCreateDialog;

	/**
	 * ...
	 * @author Giuliano Barberi
	 */
	public class MessagingIcon extends AssetIcon
	{
		private var to: String;
		private var icon: MovieClip = new ICON_MESSAGE();

		public function MessagingIcon(to: String)
		{
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

package src.UI
{
	import flash.events.Event;
	import flash.events.KeyboardEvent;
	import flash.events.MouseEvent;
	import flash.ui.Keyboard;
	import org.aswing.AbstractButton;
	import org.aswing.AssetIcon;
	import org.aswing.JButton;
	import org.aswing.JFrame;
	import org.aswing.skinbuilder.SkinCustomIcon;
	import src.Constants;
	import src.UI.LookAndFeel.GameLookAndFeel;

	public class GameJFrame extends JFrame
	{
		
		private var onDispose: Function;
		private var closeButton: JButton;

		public function GameJFrame(owner: * = null, title: String = "", modal: Boolean = true, dispose: Function = null)
		{
			super(owner, title, modal);

			onDispose = dispose;
			
			addEventListener(MouseEvent.MOUSE_WHEEL, function(e: MouseEvent): void {
				e.stopImmediatePropagation();
			});			
			
			addEventListener(MouseEvent.MOUSE_DOWN, function(e: MouseEvent): void {
				e.stopImmediatePropagation();
			});
			
			modalMC.addEventListener(MouseEvent.MOUSE_DOWN, function(e: Event): void {
				e.stopImmediatePropagation();
			});			
			
			addEventListener(KeyboardEvent.KEY_DOWN, function(e: KeyboardEvent): void {
				if (e.keyCode == Keyboard.ESCAPE) {
					if (getTitleBar() && isClosable() && getTitleBar().getCloseButton()) {						
						getTitleBar().getCloseButton().doClick();
						e.stopImmediatePropagation();
					}
				}				
			});			
			
			getTitleBar().setMaximizeButton(null);
			getTitleBar().setRestoreButton(null);
			getTitleBar().setIconifiedButton(null);	
		}		
		
		
		override public function dispose():void
		{
			super.dispose();
			if (onDispose != null) onDispose();
		}
		
		public function resizeToContents():void
		{
			setPreferredHeight(Math.min(getHeight(), Constants.screenH));
			pack();
		}
	}
}

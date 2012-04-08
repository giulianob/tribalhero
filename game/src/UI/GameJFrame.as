package src.UI
{
	import flash.events.Event;
	import flash.events.MouseEvent;
	import org.aswing.JFrame;
	import src.Constants;

	public class GameJFrame extends JFrame
	{

		private var onDispose: Function;

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

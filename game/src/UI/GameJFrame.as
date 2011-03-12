package src.UI
{
	import org.aswing.JFrame;

	/**
	 * ...
	 * @author Giuliano
	 */
	public class GameJFrame extends JFrame
	{

		private var onDispose: Function;

		public function GameJFrame(owner: * = null, title: String = "", modal: Boolean = true, dispose: Function = null)
		{
			super(owner, title, modal);

			onDispose = dispose;
		}

		override public function dispose():void
		{
			super.dispose();
			if (onDispose != null) onDispose();
		}
	}
}

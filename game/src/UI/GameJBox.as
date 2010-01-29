package src.UI
{
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;

	public class GameJBox extends JPanel
	{
		protected var frame: GameJFrame = null;

		public function getFrame(): GameJFrame {
			return frame;
		}

		public function show(owner:* = null):JFrame
		{
			if (!frame)
			{
				frame = new GameJFrame(owner, "", false);
				frame.setContentPane(this);
				frame.pack();
			}

			frame.setBackgroundDecorator(new GameJBoxBackground());
			frame.setTitleBar(null);
			frame.setDragable(false);
			frame.setClosable(false);
			frame.setResizable(false);
			frame.show();

			return frame;
		}
	}

}


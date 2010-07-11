package src.UI
{
	import org.aswing.border.EmptyBorder;
	import org.aswing.event.FrameEvent;
	import org.aswing.geom.IntPoint;
	import org.aswing.Insets;
	import org.aswing.JFrame;
	import org.aswing.JPanel;
	import src.Constants;
	import src.UI.LookAndFeel.GameLookAndFeel;

	public class GameJSidebar extends JPanel
	{
		protected var frame: GameJFrame = null;

		public static const FULL_HEIGHT: int = Constants.screenH - 105;

		public function GameJSidebar()
		{
			setPreferredWidth(175);
		}

		public function getFrame(): GameJFrame {
			return frame;
		}

		public function showSelf(owner: * = null, onClose: Function = null, onDispose: Function = null) : JFrame {
			frame = new GameJFrame(owner, "", false, onDispose);
			GameLookAndFeel.changeClass(frame, "Sidebar.frame");
			frame.setContentPane(this);
			setBorder(new EmptyBorder(getBorder(), new Insets(15, 22, 35, 25)));

			var self: JPanel = this;

			frame.addEventListener(FrameEvent.FRAME_CLOSING, function(e: FrameEvent):void { if (onClose != null) onClose(self); } );

			frame.pack();

			frame.setResizable(false);
			frame.setDragable(false);
			frame.getTitleBar().setCloseButton(null);

			frame.setLocation(new IntPoint(Constants.screenW - frame.getWidth() - 5, 60));

			return frame;
		}

		public function show(owner:* = null, onClose:Function = null):JFrame
		{
			return null;
		}
	}
}

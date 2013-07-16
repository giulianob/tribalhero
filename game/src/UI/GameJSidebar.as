package src.UI
{
    import flash.events.Event;
    import flash.events.MouseEvent;

    import org.aswing.Insets;
    import org.aswing.JFrame;
    import org.aswing.JPanel;
    import org.aswing.border.EmptyBorder;
    import org.aswing.event.FrameEvent;

    import src.Constants;
    import src.UI.LookAndFeel.GameLookAndFeel;

    public class GameJSidebar extends JPanel
	{
		protected var frame: GameJFrame = null;

		public static const FULL_HEIGHT: int = Constants.screenH - 105;
		public static const WIDTH: int = 175;

		public function GameJSidebar()
		{
			setPreferredWidth(WIDTH);

			addEventListener(MouseEvent.MOUSE_DOWN, function(e: Event): void {
				e.stopImmediatePropagation();
			});
		}

		public function getFrame(): GameJFrame {
			return frame;
		}

		public function showSelf(owner: * = null, onClose: Function = null, onDispose: Function = null) : JFrame {
			frame = new GameJFrame(owner, "", false, onDispose);
			GameLookAndFeel.changeClass(frame, "Sidebar.frame");
			frame.setContentPane(this);
			setBorder(new EmptyBorder(getBorder(), new Insets(0, 8, 5, 8)));

			var self: JPanel = this;

			frame.addEventListener(FrameEvent.FRAME_CLOSING, function(e: FrameEvent):void { if (onClose != null) onClose(self); } );

			frame.pack();

			frame.setResizable(false);
			frame.setDragable(false);
			frame.getTitleBar().setCloseButton(null);			
			frame.setLocationXY(0, 0);

			return frame;
		}

		public function show(owner:* = null, onClose:Function = null):JFrame
		{
			return null;
		}
	}
}

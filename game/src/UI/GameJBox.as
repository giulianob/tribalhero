package src.UI
{
    import org.aswing.*;
    import org.aswing.event.PopupEvent;

    public class GameJBox extends JPanel
	{
		protected var frame: GameJFrame = null;

		public function getFrame(): GameJFrame {
			return frame;
		}

		public function show(owner:* = null, hasTitle: Boolean = false):JFrame
		{
			if (!frame)
			{
				frame = new GameJFrame(owner, "", false);
				frame.setContentPane(this);
				frame.pack();
                frame.addEventListener(PopupEvent.POPUP_CLOSED, onDisposeFrame);
			}

			frame.setBackgroundDecorator(new GameJBoxBackground());
			if (!hasTitle) {
				frame.setTitleBar(null);
			}
			frame.setDragable(false);
			frame.setClosable(false);
			frame.setResizable(false);
			frame.show();

			return frame;
		}
        
        private function onDisposeFrame(e: PopupEvent): void {
            var closingFrame: JFrame = e.target as JFrame;
            closingFrame.removeEventListener(PopupEvent.POPUP_CLOSED, onDisposeFrame);
            frame = null;
        }
	}

}


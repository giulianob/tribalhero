package src.UI
{
	import flash.display.DisplayObject;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.event.FrameEvent;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import src.Util.Util;

	public class GameJImagePanel extends JPanel
	{
		protected var frame: GameJFrame = null;
		protected var imageBackground: DisplayObject;
		protected var title: String = "";

		public function GameJImagePanel(imageBackground: DisplayObject, layout: LayoutManager = null) {
			super(layout);

			this.imageBackground = imageBackground;
		}

		public function getFrame(): GameJFrame {
			return frame;
		}

		public function showSelf(owner: * = null, modal: Boolean = true, onClose: Function = null, onDispose: Function = null) : JFrame {
			frame = new GameJFrame(owner, title, modal, onDispose);
			
			frame.setBackgroundDecorator(new GameJImagePanelBackground(imageBackground));

			frame.setContentPane(this);

			var self: JPanel = this;
			frame.addEventListener(FrameEvent.FRAME_CLOSING, function(e: FrameEvent):void { if (onClose != null) onClose(self); });

			frame.pack();
			Util.centerFrame(frame);
			frame.setResizable(false);
			return frame;
		}
	}

}


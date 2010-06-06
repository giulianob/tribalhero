package src.UI 
{
	import org.aswing.ASColor;
	import org.aswing.border.EmptyBorder;
	import org.aswing.event.FrameEvent;
	import org.aswing.event.PopupEvent;
	import org.aswing.Insets;
	import org.aswing.JFrame;
	import org.aswing.JPanel;
	import org.aswing.LayoutManager;
	import src.Util.Util;
	
	/**
	 * ...
	 * @author 
	 */
	public class GameJPanel extends JPanel
	{
		protected var frame: GameJFrame = null;
		protected var title: String = "";
		
		public function GameJPanel() 
		{						
		}
		
		public function getFrame(): GameJFrame {
			return frame;
		}	
		
		public function showSelf(owner: * = null, modal: Boolean = true, onClose: Function = null, onDispose: Function = null) : JFrame {			
			frame = new GameJFrame(owner, title, modal, onDispose);
			setBorder(new EmptyBorder(getBorder(), new Insets(3, 10, 3, 10)));
			
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
package src.UI 
{
	import org.aswing.ASColor;
	import org.aswing.border.EmptyBorder;
	import org.aswing.event.FrameEvent;
	import org.aswing.event.PopupEvent;
	import org.aswing.geom.IntDimension;
	import org.aswing.geom.IntPoint;
	import org.aswing.Insets;
	import org.aswing.JFrame;
	import org.aswing.JPanel;
	import org.aswing.LayoutManager;
	import src.Constants;
	import src.Global;
	import src.Util.Util;
	
	/**
	 * ...
	 * @author 
	 */
	public class GameJSidebar extends JPanel
	{
		protected var frame: GameJFrame = null;
		
		public static const FULL_HEIGHT: int = Constants.screenH - 70;
		
		public function GameJSidebar() 
		{		
			setPreferredWidth(175);
		}
		
		public function getFrame(): GameJFrame {
			return frame;
		}		
		
		public function showSelf(owner: * = null, onClose: Function = null, onDispose: Function = null) : JFrame {			
			frame = new GameJFrame(owner, "", false, onDispose);						
			frame.setContentPane(this);
			
			var self: JPanel = this;
			
			frame.addEventListener(FrameEvent.FRAME_CLOSING, function(e: FrameEvent):void { if (onClose != null) onClose(self); } );
			
			frame.pack();
			
			frame.setResizable(false);
			frame.setDragable(false);			
			frame.getTitleBar().setCloseButton(null);			
			
			frame.setLocation(new IntPoint(Constants.screenW - frame.getWidth() + 3, 35));
			
			return frame;
		}						
		
	
		public function show(owner:* = null, onClose:Function = null):JFrame 
		{
			return null;
		}		
	}
}
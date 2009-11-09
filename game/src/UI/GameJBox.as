package src.UI 
{
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.event.FrameEvent;
	import org.aswing.event.PopupEvent;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import src.Global;
	
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
			}
			
			frame.setBackgroundDecorator(new GameJBoxBackground());
			frame.setTitleBar(null);
			frame.setDragable(false);
			frame.setClosable(false);
			frame.pack();
			frame.show();
			
			return frame;
		}
	}

}
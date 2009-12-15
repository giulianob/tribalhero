/**
* ...
* @author Default
* @version 0.1
*/

package src.UI.Tooltips {		
	import flash.display.DisplayObject;
	import flash.display.Sprite;
	import flash.display.Stage;
	import flash.events.Event;
	import flash.events.MouseEvent;	
	import flash.geom.Point;
	import org.aswing.border.EmptyBorder;
	import org.aswing.Component;
	import org.aswing.Insets;
	import org.aswing.JFrame;
	import src.Global;
	import src.UI.GameJBox;
	
	public class Tooltip
	{		
		protected var ui: GameJBox = new GameJBox();
		
		private var viewObj: DisplayObject;
		
		public function show(obj: DisplayObject):void
		{																
			var pos:Point = obj.localToGlobal(new Point(obj.x, obj.y));
			
			var objStage: Stage = obj.stage;
			
			if (this.viewObj == null)
			{
				this.viewObj = obj;						
				viewObj.addEventListener(Event.REMOVED_FROM_STAGE, parentHidden);			
			
				ui.setBorder(new EmptyBorder(null, new Insets(3, 10, 3, 10)));			
				ui.show();			
				ui.getFrame().tabEnabled = false;
				ui.getFrame().mouseEnabled = false;
			}
			else
				ui.getFrame().pack();
			
			adjustPosition();
		}		
		
		public function parentHidden(e: Event) : void {
			hide();
		}
		
		public function adjustPosition() : void
		{
			if (ui.getFrame() == null || ui.getFrame().stage == null)
				return;
			
			ui.getFrame().pack();
			
			var boxX: Number = ui.getFrame().stage.mouseX + 15;
			var boxY: Number = ui.getFrame().stage.mouseY + 15;
			
			if (boxX + ui.getFrame().getWidth() > ui.getFrame().stage.stageWidth) 
			{				
				boxX -= (boxX + ui.getFrame().getWidth()) - ui.getFrame().stage.stageWidth;
			}
			
			if (boxY + ui.getFrame().getHeight() > ui.getFrame().stage.stageHeight) 
				boxY -= (boxY + ui.getFrame().getHeight()) - ui.getFrame().stage.stageHeight;
			
			ui.getFrame().setLocationXY(boxX, boxY);			
		}
		
		public function hide():void
		{
			if (this.viewObj != null)
			{
				this.viewObj.removeEventListener(Event.REMOVED_FROM_STAGE, parentHidden);
				this.viewObj = null;
			}
			
			if (ui.getFrame())
				ui.getFrame().dispose();	
		}
	}
	
}

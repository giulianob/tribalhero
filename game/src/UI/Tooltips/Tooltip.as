/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.UI.Tooltips {
	import flash.display.DisplayObject;
	import flash.display.Stage;
	import flash.events.Event;
	import flash.geom.Point;
	import org.aswing.border.EmptyBorder;
	import org.aswing.Insets;
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
				ui.show(null);				
				ui.getFrame().parent.mouseEnabled = false;
				ui.getFrame().parent.mouseChildren = false;
				ui.getFrame().pack();
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

			var mouseX: Number = ui.getFrame().stage.mouseX;
			var mouseY: Number = ui.getFrame().stage.mouseY;
			
			var boxX: Number = mouseX;
			var boxY: Number = mouseY;
			
			var boxWidth: Number = ui.getFrame().getWidth();
			var boxHeight: Number = ui.getFrame().getHeight();
			
			var stageWidth: Number = ui.getFrame().stage.stageWidth;
			var stageHeight: Number = ui.getFrame().stage.stageHeight;
			
			if (boxX + boxWidth > stageWidth) {				
				boxX = mouseX - boxWidth + 5;
			}			
			
			if (boxY + boxHeight > stageHeight) {
				boxY = mouseY - boxHeight + 5;				
			}
			
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


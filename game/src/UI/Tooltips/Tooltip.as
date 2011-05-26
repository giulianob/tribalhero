/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.UI.Tooltips {
	import flash.display.DisplayObject;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import org.aswing.border.EmptyBorder;
	import org.aswing.Component;
	import org.aswing.event.AWEvent;
	import org.aswing.geom.IntPoint;
	import org.aswing.Insets;
	import src.UI.GameJBox;

	public class Tooltip
	{
		protected var ui: GameJBox = new GameJBox();

		protected var viewObj: DisplayObject;

		public function Tooltip() {
			ui.setBorder(new EmptyBorder(null, new Insets(3, 10, 3, 10)));
		}

		public function getUI(): GameJBox {
			return ui;
		}

		public function show(obj: DisplayObject):void
		{
			if (this.viewObj == null || this.viewObj != obj)
			{
				hide();

				this.viewObj = obj;
				viewObj.addEventListener(Event.REMOVED_FROM_STAGE, parentHidden);
				viewObj.addEventListener(MouseEvent.MOUSE_DOWN, parentHidden);

				ui.addEventListener(AWEvent.PAINT, onPaint);
				ui.show();
				ui.getFrame().pack();
				ui.getFrame().parent.mouseEnabled = false;
				ui.getFrame().parent.mouseChildren = false;

				adjustPosition();
			}
			else {
				adjustPosition();
			}
		}

		// We need this function since the size is wrong of the component until it has been painted
		private function onPaint(e: AWEvent): void {
			ui.removeEventListener(AWEvent.PAINT, onPaint);
			adjustPosition();
		}

		private function parentHidden(e: Event) : void {
			hide();
		}

		private function adjustPosition() : void
		{
			if (ui.getFrame() == null || ui.getFrame().stage == null || ui.getComBounds().width == 0) {
				return;
			}

			if (viewObj is Component) {
				(viewObj as Component).requestFocus();
			}

			var mouseX: Number = ui.getFrame().stage.mouseX;
			var mouseY: Number = ui.getFrame().stage.mouseY;

			var boxX: Number = mouseX;
			var boxY: Number = mouseY;

			var boxWidth: Number = ui.getFrame().getPreferredWidth();
			var boxHeight: Number = ui.getFrame().getPreferredHeight();

			var stageWidth: Number = ui.getFrame().stage.stageWidth;
			var stageHeight: Number = ui.getFrame().stage.stageHeight;

			if (boxX + boxWidth > stageWidth) {
				boxX = mouseX - boxWidth + 5;
			}

			if (boxY + boxHeight > stageHeight) {
				boxY = mouseY - boxHeight + 5;
			}

			if (boxY < 0) {
				boxY = 0;
			}

			if (boxX < 0) {
				boxX = 0;
			}

			ui.getFrame().setGlobalLocation(new IntPoint(boxX, boxY));
			ui.getFrame().revalidate();			
		}

		public function hide():void
		{
			if (this.viewObj != null)
			{
				this.viewObj.removeEventListener(Event.REMOVED_FROM_STAGE, parentHidden);
				this.viewObj.removeEventListener(MouseEvent.MOUSE_DOWN, parentHidden);
				this.viewObj = null;
			}

			if (ui.getFrame())
			ui.getFrame().dispose();
		}
	}

}


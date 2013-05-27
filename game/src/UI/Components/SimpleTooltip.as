package src.UI.Components 
{
	import flash.display.DisplayObject;
	import flash.display.SimpleButton;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import org.aswing.AssetPane;
	import org.aswing.Component;
	import src.UI.Tooltips.TextTooltip;
	
	public class SimpleTooltip
	{
		private var ui: DisplayObject;
		private var tooltip: TextTooltip;
		private var enabled: Boolean = true;
		
		public function SimpleTooltip(ui: DisplayObject, tooltip: String = "", header: String = "")
		{		
			this.ui = ui;
			this.tooltip = new TextTooltip(tooltip, header);
			ui.addEventListener(MouseEvent.MOUSE_MOVE, onRollOver);
			ui.addEventListener(MouseEvent.ROLL_OUT, onRollOut);
			ui.addEventListener(Event.REMOVED_FROM_STAGE, parentHidden);
		}
		
		private function parentHidden(e: Event): void {
			onRollOut(e);
			ui.removeEventListener(Event.REMOVED_FROM_STAGE, parentHidden);
		}
		
		public function setEnabled(enable: Boolean): void {
			this.enabled = enable;
		}
		
		public function setText(tooltip: String, header: String = "") : void {
			this.tooltip.hide();
			this.tooltip = new TextTooltip(tooltip, header);
		}
		
		public function append(label: Component): void {
			this.tooltip.append(label);
		}
		
		private function onRollOver(e: Event):void {
			if (enabled) {
				tooltip.show(ui);
			}
		}
		
		private function onRollOut(e: Event):void {
			tooltip.hide();
		}
	}
	
}
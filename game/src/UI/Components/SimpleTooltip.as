﻿package src.UI.Components 
{
	import flash.display.DisplayObject;
	import flash.display.SimpleButton;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import org.aswing.AssetPane;
	import src.UI.Tooltips.TextTooltip;
	
	/**
	 * ...
	 * @author 
	 */
	public class SimpleTooltip
	{
		private var ui: DisplayObject;
		private var tooltip: TextTooltip;
		
		public function SimpleTooltip(ui: DisplayObject, tooltip: String = "")
		{		
			this.ui = ui;
			this.tooltip = new TextTooltip(tooltip);
			ui.addEventListener(MouseEvent.MOUSE_MOVE, onRollOver);
			ui.addEventListener(MouseEvent.ROLL_OUT, onRollOut);
			ui.addEventListener(Event.REMOVED_FROM_STAGE, parentHidden);
		}
		
		private function parentHidden(e: Event): void {
			onRollOut(e);
			ui.removeEventListener(Event.REMOVED_FROM_STAGE, parentHidden);
		}
		
		public function setText(tooltip: String) : void {
			this.tooltip.hide();
			this.tooltip = new TextTooltip(tooltip);
		}
		
		private function onRollOver(e: Event):void {
			tooltip.show(ui);
		}
		
		private function onRollOut(e: Event):void {
			tooltip.hide();
		}
	}
	
}
package src.UI.Components 
{
	import flash.display.SimpleButton;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import src.UI.Tooltips.TextTooltip;
	
	/**
	 * ...
	 * @author 
	 */
	public class SimpleTooltipButton extends SimpleButton
	{
		var tooltip: TextTooltip;
		
		public function SimpleTooltipButton() 
		{
			
		}
		
		public function init(tooltip: String, onClick: Function) {
			this.tooltip = new TextTooltip(tooltip);
			addEventListener(MouseEvent.CLICK, onClick);
			addEventListener(MouseEvent.ROLL_OVER, onRollOver);
			addEventListener(MouseEvent.ROLL_OUT, onRollOut);
		}
		
		function onRollOver(e: Event) {
			tooltip.show(stage, this);
		}
		
		function onRollOut(e: Event) {
			tooltip.hide();
		}
	}
	
}
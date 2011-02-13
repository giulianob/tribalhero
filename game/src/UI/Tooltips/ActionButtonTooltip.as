package src.UI.Tooltips 
{
	import flash.display.DisplayObject;
	/**
	 * ...
	 * @author Giuliano Barberi
	 */
	public class ActionButtonTooltip extends Tooltip
	{

		protected var drawTooltip: Boolean = false;
		
		public function ActionButtonTooltip() 
		{
			
		}
		
		override public function show(obj:DisplayObject):void 
		{
			drawTooltip = true;
			draw(0, 0);
			super.show(obj);
		}
		
		override public function hide():void 
		{
			super.hide();
			drawTooltip = false;
		}
		
		public function draw(count: int, max: int) : void {}
		
	}

}
package src.UI.Tooltips 
{
    public class ActionButtonTooltip extends Tooltip
	{
		protected var drawTooltip: Boolean = false;
		
		public function ActionButtonTooltip() {	
		}
		
		override public function show(obj: *):void
		{
			drawTooltip = true;
			draw();
			super.show(obj);
		}
		
		override public function hide():void 
		{
			super.hide();
			drawTooltip = false;
		}
		
		public function draw() : void {}
		
	}

}
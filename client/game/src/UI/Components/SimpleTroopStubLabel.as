package src.UI.Components 
{
    import flash.events.*;

    import org.aswing.*;

    import src.Objects.Troop.*;
    import src.UI.Tooltips.*;

    public class SimpleTroopStubLabel extends JLabelButton
	{
		private var tooltip: SimpleTroopStubTooltip;
		
		public function SimpleTroopStubLabel(label: String, stub: TroopStub) 
		{
			super(label);
			
			var self: JLabelButton = this;

			addEventListener(MouseEvent.MOUSE_OVER, function(e: Event): void {
				if (tooltip) {
					tooltip.hide();
				}
				
				tooltip = new SimpleTroopStubTooltip(stub);
				tooltip.show(self);
			});
			
			addEventListener(MouseEvent.MOUSE_OUT, function(e: Event): void {
				if (tooltip) {
					tooltip.hide();
					tooltip = null;
				}
			});
		}
		
	}

}
package src.UI.Components 
{
	import flash.events.Event;
	import flash.events.MouseEvent;
	import org.aswing.event.InteractiveEvent;
	import org.aswing.JScrollPane;

	public class StickyScroll 
	{		
		public function StickyScroll(scrollview: JScrollPane) 
		{
			var stickScroll:Boolean = true;
			var lastScrollValue:int = 0;
			scrollview.addAdjustmentListener(function(e:InteractiveEvent):void
				{
					if (e.isProgrammatic())
					{
						scrollview.getVerticalScrollBar().setValue(stickScroll ? scrollview.getVerticalScrollBar().getMaximum() : lastScrollValue, false);
					}
					else
					{
						stickScroll = scrollview.getVerticalScrollBar().getValue() == scrollview.getVerticalScrollBar().getModel().getMaximum() - scrollview.getVerticalScrollBar().getModel().getExtent();
						lastScrollValue = scrollview.getVerticalScrollBar().getValue();
					}
				});
				
			scrollview.getViewport().getViewportPane().addEventListener(MouseEvent.MOUSE_WHEEL, function(e:Event):void
				{
					scrollview.getVerticalScrollBar().dispatchEvent(e);
				});
				
		}
		
	}

}
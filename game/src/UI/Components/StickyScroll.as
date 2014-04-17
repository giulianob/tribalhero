package src.UI.Components 
{
    import flash.events.Event;
    import flash.events.MouseEvent;

    import org.aswing.JScrollPane;
    import org.aswing.event.InteractiveEvent;

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
                        var newScrollValue: int = stickScroll ? scrollview.getVerticalScrollBar().getMaximum() : lastScrollValue;
                        
                        if (scrollview.getVerticalScrollBar().getValueIsAdjusting() || newScrollValue == scrollview.getVerticalScrollBar().getValue()) {
                            return;
                        }
                        
						scrollview.getVerticalScrollBar().setValue(newScrollValue, false);
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
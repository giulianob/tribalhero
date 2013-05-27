package src.UI.Components.ScreenMessages
{
	import flash.display.DisplayObjectContainer;
	import flash.events.Event;
    import flash.events.KeyboardEvent;
    import flash.events.MouseEvent;
	import flash.events.TimerEvent;
	import flash.utils.Timer;
	import org.aswing.border.EmptyBorder;
	import org.aswing.geom.IntDimension;
	import org.aswing.Insets;
	import org.aswing.JFrame;
	import org.aswing.JPanel;
	import fl.transitions.easing.*;
	import org.aswing.SoftBoxLayout;

	/**
	 * ...
	 * @author Giuliano Barberi
	 */
	public class ScreenMessagePanel extends JPanel
	{
        private const IDLE_TIME: int = 90000;
        
		private var tempItems: Array = new Array();
		private var timer: Timer = new Timer(100, 0);
		private var frame: JFrame;
        private var lastInteraction: int;

		public function ScreenMessagePanel(owner: DisplayObjectContainer)
		{
			mouseEnabled = false;
			mouseChildren = false;

			setBorder(new EmptyBorder(null, new Insets(0, 0, 0, 0)));
			setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 2));

			timer.addEventListener(TimerEvent.TIMER, onTimer);

			setPreferredSize(new IntDimension(380, 600));

			if (!frame)
			{
				frame = new JFrame(owner, "", false);						
				frame.setContentPane(this);
				frame.setBorder(new EmptyBorder(null, new Insets(0, 0, 0, 0)));
				frame.setBackgroundDecorator(null);
				frame.setTitleBar(null);
				frame.setDragable(false);
				frame.setClosable(false);
				frame.setResizable(false);		
				frame.parent.mouseEnabled = false;
				frame.parent.mouseChildren = false;
				frame.show();

				frame.setLocationXY(5, 80);
			}

			frame.pack();
            
            stage.addEventListener(MouseEvent.MOUSE_DOWN, onInteraction, true, 0, true);            
            stage.addEventListener(KeyboardEvent.KEY_DOWN, onInteraction, true, 0, true);                                 
		}
        
        public function onInteraction(e: Event): void {
            lastInteraction = new Date().time;
        }

		public function dispose() : void {
			if (frame) {
				timer.stop();
				frame.dispose();
			}
		}

		public function addMessage(item: ScreenMessageItem) : void {
			if (hasMessage(item.key)) return;

			if (item.duration != 0) {
				tempItems.push(item);

				if (!timer.running) {
					timer.start();					
				}
			}

			append(item);			
		}
		
		public function hasMessage(key: String) : Boolean {
			for (var i: int = getComponentCount() - 1; i >= 0; i--) {
				var item: ScreenMessageItem = getComponent(i) as ScreenMessageItem;
				if (item.key == key) {
					return true;
				}
			}
			
			return false;
		}

		public function removeMessage(key: String) : void {
			for (var i: int = getComponentCount() - 1; i >= 0; i--) {
				var item: ScreenMessageItem = getComponent(i) as ScreenMessageItem;
				if (item.key == key) {
					removeAt(i);
					if (item.duration > 0) {
						for (var j: int = tempItems.length - 1; j >= 0; j--) {
							if (tempItems[j].key == key) {
								tempItems.splice(j, 1);
								break;
							}
						}
					}

					break;
				}
			}
		}

		public function onTimer(e: Event) : void {
            
            var now: int = new Date().time;
            if (now - lastInteraction > IDLE_TIME) {
                return;
            }
            
			for (var i: int = tempItems.length - 1; i >= 0; i--) {
				var item: ScreenMessageItem = tempItems[i];
				item.duration -= timer.delay;
				if (item.duration <= 0) {
					item.alpha -= 0.05;

					if (item.alpha < 0) {
						tempItems.splice(i, 1);
						remove(item);
					}
				}
			}

			if (tempItems.length == 0) {
				timer.stop();
			}
		}

	}

}


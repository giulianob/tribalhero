package src.UI.Tooltips {
	import com.greensock.loading.core.DisplayObjectLoader;
	import flash.display.DisplayObject;
	import flash.display.InteractiveObject;
	import flash.events.Event;
    import flash.events.EventDispatcher;
    import flash.events.IEventDispatcher;
	import flash.events.MouseEvent;
	import org.aswing.AsWingManager;
	import org.aswing.border.EmptyBorder;
	import org.aswing.Component;
    import org.aswing.Container;
	import org.aswing.event.AWEvent;
    import org.aswing.event.ContainerEvent;
    import org.aswing.event.PopupEvent;
	import org.aswing.FocusManager;
	import org.aswing.geom.IntPoint;
	import org.aswing.Insets;
    import org.aswing.JPanel;
	import src.Global;
	import src.Map.Camera;
	import src.UI.GameJBox;
    import src.UI.GameJBoxBackground;

	public class Tooltip implements IEventDispatcher
	{
        private var dispatcher: EventDispatcher;
        
        protected var container: JPanel = new JPanel();
        
		protected var ui: JPanel = new JPanel();

		protected var viewObj: DisplayObject;
		
		private var position: IntPoint;
        
        private var needsUpdate: Boolean = false;

		public function Tooltip() {                        
            this.dispatcher = new EventDispatcher(this);
			container.setBorder(new EmptyBorder(null, new Insets(3, 10, 3, 10)));		
            container.setOpaque(true);
            container.setBackgroundDecorator(new GameJBoxBackground());
            container.append(ui);
            ui.addEventListener(ContainerEvent.COM_ADDED, function(e: Event): void {
                resize();
            });
            container.addEventListener(AWEvent.PAINT, function(e: Event): void {                
                adjustPosition(); 
            });                        
		}

		public function getUI(): JPanel {
			return ui;
		}
		
		public function bind(obj: DisplayObject) : void {
			obj.addEventListener(MouseEvent.MOUSE_MOVE, function(e: Event): void {
				show(obj);
			});
			obj.addEventListener(MouseEvent.MOUSE_OUT, function(e: Event): void {
				hide();
			});			
		}

		public function show(obj: DisplayObject):void
		{
			this.position = new IntPoint(Global.map.stage.mouseX, Global.map.stage.mouseY);
						
			Global.map.camera.addEventListener(Camera.ON_MOVE, onCameraMove, false, 0, true);
			
			if (this.viewObj == null || this.viewObj != obj) {
				this.viewObj = obj;
				viewObj.addEventListener(Event.REMOVED_FROM_STAGE, parentHidden);
				viewObj.addEventListener(MouseEvent.MOUSE_DOWN, parentHidden);				
				
				showFrame(obj);
                
                AsWingManager.callLater(resize, 10);                
			}
			
            this.needsUpdate = true;
		}
        
        public function resize(): void {  
            trace("RESIZE");
            container.pack();
            container.repaintAndRevalidate();
        }
        
		public function showFixed(position: IntPoint):void
		{			
			this.position = position;
			showFrame();			
		}
		
		protected function showFrame(obj: DisplayObject = null): void {		
            Global.gameContainer.stage.addChild(container);
            Global.gameContainer.stage.addEventListener(Event.ENTER_FRAME, enterFrame);
            
            if (!mouseInteractive()) {
                container.mouseEnabled = false;
                container.mouseChildren = false;
                container.tabEnabled = false;
            }            
            
            resize();
		}
		
		protected function mouseInteractive(): Boolean {
			return false;
		}
		
		private function onCameraMove(e: Event): void {
			// Hide if camera is moving
			hide();
		}

		private function parentHidden(e: Event) : void {
			hide();
		}

		private function adjustPosition() : void
		{                                          
			if (container.stage == null || container.getComBounds().width == 0) {
				return;
			}

			var posX: Number = position.x;
			var posY: Number = position.y;

			var boxX: Number = posX;
			var boxY: Number = posY;

			var boxWidth: Number = ui.getPreferredWidth();
			var boxHeight: Number = ui.getPreferredHeight();

			var stageWidth: Number = container.stage.stageWidth;
			var stageHeight: Number = container.stage.stageHeight;

			if (boxX + boxWidth > stageWidth) {
				boxX = posX - boxWidth + 5;
			}

			if (boxY + boxHeight > stageHeight) {
				boxY = posY - boxHeight + 5;
			}

			if (boxY < 0) {
				boxY = 0;
			}

			if (boxX < 0) {
				boxX = 0;
			}

			container.setGlobalLocation(new IntPoint(boxX, boxY));
		}

		public function hide():void
		{
			Global.map.camera.removeEventListener(Camera.ON_MOVE, onCameraMove);			                                  
            
			if (this.viewObj != null)
			{
				this.viewObj.removeEventListener(Event.REMOVED_FROM_STAGE, parentHidden);
				this.viewObj.removeEventListener(MouseEvent.MOUSE_DOWN, parentHidden);
				this.viewObj = null;
			}

			if (container.stage) {
                container.stage.removeChild(container);
                Global.gameContainer.stage.removeEventListener(Event.ENTER_FRAME, enterFrame);
            }
            
            dispatchEvent(new Event(PopupEvent.POPUP_CLOSED));
		}
        
        private function enterFrame(e:Event):void 
        {
            if (!this.needsUpdate) {
                return;
            }
            
            this.needsUpdate = false;
            adjustPosition();
        }
        
		public function addEventListener(type:String, listener:Function, useCapture:Boolean = false, priority:int = 0, useWeakReference:Boolean = false):void{
			dispatcher.addEventListener(type, listener, useCapture, priority);
		}

		public function dispatchEvent(evt: Event):Boolean{
			return dispatcher.dispatchEvent(evt);
		}

		public function hasEventListener(type:String):Boolean{
			return dispatcher.hasEventListener(type);
		}

		public function removeEventListener(type:String, listener:Function, useCapture:Boolean = false):void{
			dispatcher.removeEventListener(type, listener, useCapture);
		}

		public function willTrigger(type:String):Boolean {
			return dispatcher.willTrigger(type);
		}	        
	}

}


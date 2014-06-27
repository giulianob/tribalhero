package src.UI.Tooltips {

    import flash.display.*;
    import flash.events.*;

    import org.aswing.AsWingManager;
    import org.aswing.Insets;
    import org.aswing.JPanel;
    import org.aswing.border.EmptyBorder;
    import org.aswing.event.AWEvent;
    import org.aswing.event.ContainerEvent;
    import org.aswing.geom.IntPoint;

    import src.Global;
    import src.Map.Camera;
    import src.UI.GameJBoxBackground;

    import starling.display.*;
    import starling.events.*;

    public class Tooltip
	{
        protected var ui: JPanel = new JPanel();

        private var container: JPanel = new JPanel();
        
		private var viewObj: *;
		
		private var position: IntPoint;
        
        private var needsUpdate: Boolean = false;

		public function Tooltip() {                        
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

		public function show(obj: *):void
		{
            this.position = new IntPoint(Global.stage.mouseX, Global.stage.mouseY);

            if (Global.map) {
                Global.map.camera.addEventListener(Camera.ON_MOVE, onCameraMove, false, 0, true);
            }

			if (this.viewObj == null || this.viewObj != obj) {
                unregisterListeners();

				this.viewObj = obj;

                registerListeners();

				showFrame();
                AsWingManager.callLater(resize, 10);                
			}
			
            this.needsUpdate = true;
		}

        private function registerListeners(): void {
            var flashSprite: flash.display.DisplayObject = viewObj as flash.display.DisplayObject;
            if (flashSprite) {
                flashSprite.addEventListener(flash.events.Event.REMOVED_FROM_STAGE, parentHidden);
                flashSprite.addEventListener(MouseEvent.MOUSE_DOWN, parentHidden);
                return;
            }

            var starlingSprite: starling.display.DisplayObject = viewObj as starling.display.DisplayObject;
            if (starlingSprite) {
                starlingSprite.addEventListener(starling.events.Event.REMOVED_FROM_STAGE, parentHidden);
                starlingSprite.addEventListener(starling.events.TouchEvent.TOUCH, starlingTouch);
                return
            }

            throw new Error("Trying to show tooltip on an invalid object");
        }

        private function unregisterListeners(): void {
            if (!viewObj)
            {
                return;
            }

            var flashSprite: flash.display.DisplayObject = viewObj as flash.display.DisplayObject;
            if (flashSprite) {
                flashSprite.removeEventListener(Event.REMOVED_FROM_STAGE, parentHidden);
                flashSprite.removeEventListener(MouseEvent.MOUSE_DOWN, parentHidden);
                return;
            }

            var starlingSprite: starling.display.DisplayObject = viewObj as starling.display.DisplayObject;
            if (starlingSprite) {
                starlingSprite.removeEventListener(starling.events.Event.REMOVED_FROM_STAGE, parentHidden);
                starlingSprite.removeEventListener(starling.events.TouchEvent.TOUCH, starlingTouch);
                return
            }

            throw new Error("Trying to show tooltip on an invalid object");
        }

        public function resize(): void {  
            container.pack();
            container.repaintAndRevalidate();
        }
        
		public function showFixed(position: IntPoint):void
		{			
			this.position = position;
			showFrame();			
		}

        protected function showFrame(): void {
            Global.stage.addChild(container);
            Global.stage.addEventListener(Event.ENTER_FRAME, enterFrame);
            
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

        private function starlingTouch(e: starling.events.TouchEvent): void {
            var touch: Touch = e.getTouch(this.viewObj, TouchPhase.BEGAN);
            if (touch) {
                hide();
            }
        }

		private function onCameraMove(e: flash.events.Event): void {
			// Hide if camera is moving
			hide();
		}

		private function parentHidden(e: *) : void {
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
			if (Global.map) {
				Global.map.camera.removeEventListener(Camera.ON_MOVE, onCameraMove);	
			}
            
			if (this.viewObj != null)
			{
				unregisterListeners();
				this.viewObj = null;
			}

            Global.stage.removeChild(container);
            Global.stage.removeEventListener(flash.events.Event.ENTER_FRAME, enterFrame);
		}
        
        private function enterFrame(e: flash.events.Event):void
        {
            if (!this.needsUpdate) {
                return;
            }
            
            this.needsUpdate = false;
            adjustPosition();
        }
	}

}


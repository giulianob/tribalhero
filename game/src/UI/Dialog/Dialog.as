/**
* ...
* @author Default
* @version 0.1
*/

package src.UI.Dialog {
	import flash.display.DisplayObject;
	import flash.display.MovieClip;
	import flash.display.SimpleButton;
	import flash.display.Sprite;
	import flash.events.MouseEvent;
	import flash.utils.getDefinitionByName;
	import src.UI.SmartMovieClip;
	
	public class Dialog extends SmartMovieClip {
		
		public var fadeBackground: Boolean = true;
		
		protected var onAccept: Function;
		protected var onClose: Function;
		
		private var mainBg: Sprite;
		private var shadowBg: Sprite;
		
		private var dWidth: Number;
		private var dHeight: Number;
		
		private var closeBtn: SimpleButton;
		
		public function Dialog() 
		{						
			dWidth = getSmartWidth();
			dHeight = getSmartHeight();
						
			drawBorder();
		}
		
		public function resize(width: Number = -1, height: Number = -1):void
		{
			if (width < 0)			
				dWidth = getSmartWidth();
			else
				dWidth = width;
				
			if (height < 0)
				dHeight = getSmartHeight();
			else
				dHeight = height;
				
			drawBorder();		
			positionCloseButton();
		}
		
		private function drawBorder():void
		{
			if (mainBg)
				removeChild(mainBg);
				
			if (shadowBg)
				removeChild(shadowBg);
				
			mainBg = new Sprite();
			mainBg.graphics.beginFill(uint(0xFFFFFF), 1.0);
			mainBg.graphics.lineStyle(3, 0x000000);								
			mainBg.graphics.drawRoundRect(0, 0, dWidth + 40, dHeight + 40, 20, 20);
			mainBg.graphics.endFill();									
			
			mainBg.x = -20;
			mainBg.y = -20;
			
			shadowBg = new Sprite();
			shadowBg.graphics.beginFill(uint(0xAAAAAA), 0.30);			
			shadowBg.graphics.drawRoundRect(0, 0, dWidth + 40, dHeight + 40, 20, 20);
			shadowBg.graphics.endFill();						
			
			shadowBg.x = -10;
			shadowBg.y = -10;
			
			addChildAt(mainBg, 0);
			addChildAt(shadowBg, 0);			
		}
		
		protected function setOnAccept(f: Function, btn: DisplayObject = null):void
		{			
			if (btn)
				btn.addEventListener(MouseEvent.CLICK, onAcceptDialog);			
			
			onAccept = f;
		}
		
		protected function setOnClose(f: Function, btn: DisplayObject = null):void
		{			
			if (closeBtn == null)
			{
				closeBtn = new DLG_CLOSE_BUTTON() as SimpleButton;
				closeBtn.addEventListener(MouseEvent.CLICK, onCloseDialog);
				addChild(closeBtn);
				positionCloseButton();
			}			
			
			if (btn != null)
				btn.addEventListener(MouseEvent.CLICK, onCloseDialog);			
			
			onClose = f;
		}
		
		private function positionCloseButton():void
		{
			if (closeBtn == null) return;
			
			closeBtn.x = dWidth;
			closeBtn.y = 0 - closeBtn.height;
		}
		
		public function Accept():void
		{
			if (onAccept != null)
				onAccept(this);			
		}
		
		public function onAcceptDialog(event: MouseEvent):void
		{
			if (onAccept != null)
				onAccept(this);
		}
		
		public function onCloseDialog(event: MouseEvent):void
		{
			if (onClose != null)
				onClose(this);
		}
		
		public function enableInput():void
		{
			enabled = true;
			mouseChildren = true;
			tabEnabled = true;
		}
		
		public function disableInput():void
		{			
			enabled = false;
			mouseChildren = false;
			tabEnabled = false;
		}		
		
		public function hasFadedBackground(): Boolean
		{
			return fadeBackground;
		}
	}
	
}

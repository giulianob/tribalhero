
package src.UI.Sidebars.CursorCancel {
	import flash.display.MovieClip;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import flash.text.TextField;
	import src.Global;
	import src.Map.Map;
	import src.Objects.GameObject;
	import src.Constants;	
	import src.UI.GameJSidebar;
	
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	
	public class CursorCancelSidebar extends GameJSidebar {
			
		private var parentObj: GameObject;
		
		private var btnCancel: CursorCancelButton_base = new CursorCancelButton_base();
		
		public function CursorCancelSidebar(parentObj: GameObject = null) {
			createUI();
			
			btnCancel.addEventListener(MouseEvent.CLICK, onCancel);
			
			this.parentObj = parentObj;						
		}
		
		public function onCancel(event: MouseEvent):void
		{		
			Global.map.gameContainer.setOverlaySprite(null);
			Global.map.gameContainer.setSidebar(null);
			
			if (parentObj != null)
				Global.map.selectObject(parentObj);
		}		
		
		private function createUI() : void
		{
			//component creation			
			append(new AssetPane(btnCancel));
		}
		
		override public function show(owner:* = null, onClose:Function = null):JFrame 
		{
			super.showSelf(owner, onClose);
			
			frame.getTitleBar().setText("Cancel");
			
			frame.show();
			return frame;
		}			
	}
	
}

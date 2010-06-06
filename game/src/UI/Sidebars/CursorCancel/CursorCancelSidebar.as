
package src.UI.Sidebars.CursorCancel {
	import flash.events.MouseEvent;
	import src.Global;
	import src.Objects.GameObject;
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
			Global.gameContainer.setOverlaySprite(null);
			Global.gameContainer.setSidebar(null);

			if (parentObj != null)
			Global.map.selectObject(parentObj);
		}

		private function createUI() : void
		{
			//component creation
			append(new AssetPane(btnCancel));
		}

		public function dispose():void
		{
			Global.gameContainer.setOverlaySprite(null);
		}

		override public function show(owner:* = null, onClose:Function = null):JFrame
		{
			super.showSelf(owner, onClose, dispose);

			frame.getTitleBar().setText("Cancel");

			frame.show();
			return frame;
		}
	}

}


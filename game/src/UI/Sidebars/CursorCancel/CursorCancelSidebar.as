
package src.UI.Sidebars.CursorCancel {
	import flash.events.Event;
	import flash.events.MouseEvent;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import org.aswing.geom.*;
	import src.Global;
	import src.Objects.GameObject;
	import src.Objects.SimpleGameObject;
	import src.Objects.SimpleObject;
	import src.UI.Components.SimpleTooltip;
	import src.UI.GameJSidebar;


	public class CursorCancelSidebar extends GameJSidebar {

		private var parentObj: SimpleObject;

		private var btnCancel: JButton = new JButton("Cancel");

		public function CursorCancelSidebar(parentObj: SimpleObject = null) {
			createUI();

			btnCancel.addActionListener(onCancel);

			this.parentObj = parentObj;
		}

		public function onCancel(event: Event):void
		{
			Global.gameContainer.setOverlaySprite(null);
			Global.gameContainer.setSidebar(null);

			if (parentObj != null)
				Global.map.selectObject(parentObj);
		}

		private function createUI() : void
		{
			setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
			//component creation
			append(btnCancel);
		}

		public function dispose():void
		{
			Global.gameContainer.setOverlaySprite(null);
		}

		override public function show(owner:* = null, onClose:Function = null):JFrame
		{
			super.showSelf(owner, onClose, dispose);

			frame.getTitleBar().setText("Actions");

			frame.show();
			return frame;
		}
	}

}


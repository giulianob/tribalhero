
package src.UI.Sidebars.ObjectInfo.Buttons {
	import flash.events.*;
	import src.*;
	import src.Objects.*;
	import src.Objects.Actions.*;
	import src.Objects.Factories.*;
	import src.UI.Cursors.*;
	import src.UI.Tooltips.*;

	public class StructureSelfDestroyButton extends ActionButton
	{
		private var tooltip: TextTooltip;

		public function StructureSelfDestroyButton(parentObj: SimpleGameObject)
		{
			super(parentObj, "Remove");

			tooltip = new TextTooltip("Remove this structure");

			addEventListener(MouseEvent.CLICK, onMouseClick);
			addEventListener(MouseEvent.MOUSE_OVER, onMouseOver);
			addEventListener(MouseEvent.MOUSE_MOVE, onMouseOver);
			addEventListener(MouseEvent.MOUSE_OUT, onMouseOut);
		}

		public function onMouseOver(event: MouseEvent):void
		{
			tooltip.show(this);
		}

		public function onMouseOut(event: MouseEvent):void
		{
			tooltip.hide();
		}

		public function onMouseClick(event: Event):void
		{
			if (isEnabled())			
				Global.mapComm.Objects.structureSelfDestroy(parentObj.groupId, parentObj.objectId);			

			event.stopImmediatePropagation();
		}
	}

}


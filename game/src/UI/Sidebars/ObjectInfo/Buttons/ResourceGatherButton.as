
package src.UI.Sidebars.ObjectInfo.Buttons {
	import flash.events.Event;
	import flash.events.MouseEvent;
	import flash.geom.Point;
	import src.Global;
	import src.Map.MapUtil;
	import src.Objects.Factories.*;
	import src.Objects.GameObject;
	import src.Objects.Actions.ActionButton;
	import src.Objects.SimpleGameObject;
	import src.Objects.States.MovingState;
	import src.UI.Cursors.*;
	import src.UI.Tooltips.TextTooltip;

	public class ResourceGatherButton extends ActionButton
	{
		private var tooltip: TextTooltip;

		public function ResourceGatherButton(parentObj: SimpleGameObject)
		{
			super(parentObj, "Harvest");

			tooltip = new TextTooltip("Harvest the resources");

			addEventListener(MouseEvent.CLICK, onMouseClick);
			addEventListener(MouseEvent.MOUSE_OVER, onMouseOver);
			addEventListener(MouseEvent.MOUSE_MOVE, onMouseOver);
			addEventListener(MouseEvent.MOUSE_OUT, onMouseOut);
		}

		// Override disable since this button can always be clicked
		override public function disable():void
		{
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
				Global.mapComm.Object.gatherResource(parentObj.groupId, parentObj.objectId);

			event.stopImmediatePropagation();
		}
	}

}


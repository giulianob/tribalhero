
package src.UI.Sidebars.ObjectInfo.Buttons {
	import flash.display.SimpleButton;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import src.Global;
	import src.Objects.Factories.*;
	import src.Objects.*;
	import src.Objects.Actions.ActionButton;
	import src.UI.Components.SimpleTooltip;
	import src.UI.Cursors.*;

	public class SendResourcesButton extends ActionButton
	{
		public function SendResourcesButton(parentObj: GameObject)
		{
			super(parentObj, "Send Resources");

			new SimpleTooltip(this, "Send resources to other players.");

			addEventListener(MouseEvent.CLICK, onMouseClick);
		}

		public function onMouseClick(MouseEvent: Event):void
		{
			if (isEnabled())
			{
				var cursor: SendResourcesCursor = new SendResourcesCursor();
				cursor.init(parentObj.cityId, parentObj as StructureObject);
			}
		}

		override public function validateButton(): Boolean
		{
			return isEnabled();
		}
	}

}


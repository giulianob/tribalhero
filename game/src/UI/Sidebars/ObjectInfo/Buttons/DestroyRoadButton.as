
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

	public class DestroyRoadButton extends ActionButton
	{
		public function DestroyRoadButton(button: SimpleButton, parentObj: GameObject)
		{
			super(button, parentObj);

			new SimpleTooltip(button, "Destroy Road");

			ui.addEventListener(MouseEvent.CLICK, onMouseClick);
		}

		public function onMouseClick(MouseEvent: Event):void
		{
			if (enabled)
			{
				var cursor: DestroyRoadCursor = new DestroyRoadCursor();
				cursor.init(Global.map, parentObj);
			}
		}

		override public function validateButton(): Boolean
		{
			return true;
		}
	}

}


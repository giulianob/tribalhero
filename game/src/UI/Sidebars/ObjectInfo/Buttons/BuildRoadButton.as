
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

	public class BuildRoadButton extends ActionButton
	{
		public function BuildRoadButton(button: SimpleButton, parentObj: GameObject)
		{
			super(button, parentObj);

			new SimpleTooltip(button, "Build Road");

			ui.addEventListener(MouseEvent.CLICK, onMouseClick);
		}

		public function onMouseClick(MouseEvent: Event):void
		{
			if (enabled)
			{
				var cursor: BuildRoadCursor = new BuildRoadCursor();
				cursor.init(Global.map, parentObj);
			}
		}

		override public function validateButton(): Boolean
		{
			return true;
		}
	}

}


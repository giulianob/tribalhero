
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
		public function BuildRoadButton(parentObj: SimpleGameObject)
		{
			super(parentObj, "Build Road");

			new SimpleTooltip(this, "Build Road");

			addEventListener(MouseEvent.CLICK, onMouseClick);
		}

		public function onMouseClick(MouseEvent: Event):void
		{
			if (isEnabled())
			{
				var cursor: BuildRoadCursor = new BuildRoadCursor();
				cursor.init(parentObj);
			}
		}

		override public function alwaysEnabled(): Boolean
		{
			return true;
		}
	}

}


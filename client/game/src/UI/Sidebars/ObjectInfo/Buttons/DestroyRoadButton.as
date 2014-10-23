
package src.UI.Sidebars.ObjectInfo.Buttons {
    import flash.events.Event;
    import flash.events.MouseEvent;

    import src.Objects.*;
    import src.FeathersUI.Controls.ActionButton;
    import src.UI.Components.SimpleTooltip;
    import src.UI.Cursors.*;

    public class DestroyRoadButton extends ActionButton
	{
		public function DestroyRoadButton(parentObj: SimpleGameObject)
		{
			super(parentObj, "Destroy Road");

			// new SimpleTooltip(this, "Destroy Road");

			addEventListener(MouseEvent.CLICK, onMouseClick);
		}

		public function onMouseClick(MouseEvent: Event):void
		{
			if (isEnabled)
			{
				var cursor: DestroyRoadCursor = new DestroyRoadCursor();
				cursor.init(parentObj);
			}
		}

		override public function alwaysEnabled(): Boolean
		{
			return true;
		}
	}

}


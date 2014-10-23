
package src.UI.Sidebars.ObjectInfo.Buttons {
    import flash.events.Event;
    import flash.events.MouseEvent;

    import src.Objects.*;
    import src.FeathersUI.Controls.ActionButton;
    import src.UI.Components.SimpleTooltip;
    import src.UI.Cursors.*;

    public class StructureUserDowngradeButton extends ActionButton
	{
		public function StructureUserDowngradeButton(parentObj: SimpleGameObject)
		{
			super(parentObj, "Destroy Structures");

//			new SimpleTooltip(this, "Destroy other structures. You will not receive back any resources for destroying structures.");

			addEventListener(MouseEvent.CLICK, onMouseClick);
		}

		public function onMouseClick(MouseEvent: Event):void
		{
			if (isEnabled)
			{
				var cursor: StructureDowngradeCursor = new StructureDowngradeCursor(parentObj as StructureObject);
			}
		}
	}

}



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

	public class StructureUserDowngradeButton extends ActionButton
	{
		public function StructureUserDowngradeButton(parentObj: SimpleGameObject)
		{
			super(parentObj, "Downgrade Structure");

			new SimpleTooltip(this, "Downgrades any structures in your city to one level lower.\n\nStructures may be completely removed by downgrading them back to level 0.");

			addEventListener(MouseEvent.CLICK, onMouseClick);
		}

		public function onMouseClick(MouseEvent: Event):void
		{
			if (isEnabled())
			{
				var cursor: StructureDowngradeCursor = new StructureDowngradeCursor();
				cursor.init(parentObj as StructureObject);
			}
		}
	}

}



package src.UI.Sidebars.TroopInfo.Buttons {
	import flash.events.Event;
	import flash.events.MouseEvent;
	import src.Objects.Factories.*;
	import src.Objects.GameObject;
	import src.Objects.Actions.ActionButton;
	import src.Objects.Troop.*;
	import src.UI.Cursors.*;
	import src.UI.Dialog.TroopInfoDialog;
	import src.UI.Tooltips.TextTooltip;

	public class ViewUnitsButton extends ActionButton
	{
		private var tooltip: TextTooltip;

		public function ViewUnitsButton(parentObj: GameObject)
		{
			super(parentObj, "View Units");

			tooltip = new TextTooltip("View Units");

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
			{
				var troopInfo: TroopInfoDialog = new TroopInfoDialog(parentObj as TroopObject);

				troopInfo.show();
			}

			event.stopImmediatePropagation();
		}
	}

}


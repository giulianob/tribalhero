
package src.UI.Sidebars.StrongholdInfo.Buttons {
	import flash.events.Event;
	import flash.events.MouseEvent;
	import src.Constants;
	import src.Global;
	import src.Objects.Factories.*;
	import src.Objects.GameObject;
	import src.Objects.Actions.ActionButton;
	import src.Objects.Stronghold.Stronghold;
	import src.Objects.Troop.*;
	import src.UI.Components.SimpleTooltip;
	import src.UI.Cursors.*;
	import src.UI.Dialog.TroopInfoDialog;
	import src.UI.Tooltips.TextTooltip;

	public class ViewStrongholdButton extends ActionButton
	{
		private var tooltip: TextTooltip;

		public function ViewStrongholdButton(parentObj: Stronghold)
		{
			super(parentObj, "View Profile");

			new SimpleTooltip(this, "View Profile");

			addEventListener(MouseEvent.CLICK, onMouseClick);
		}

		public function onMouseClick(event: Event):void
		{
			if (isEnabled())
			{
				var stronghold: Stronghold = parentObj as Stronghold;
				if (Constants.tribe.isInTribe(stronghold.tribeId))
				{
					Global.mapComm.Stronghold.viewStrongholdProfile(stronghold.id);
				}
			}
		}
	}
}


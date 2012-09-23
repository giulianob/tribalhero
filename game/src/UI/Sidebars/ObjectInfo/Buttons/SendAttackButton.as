
package src.UI.Sidebars.ObjectInfo.Buttons {
	import flash.events.Event;
	import flash.events.MouseEvent;
	import src.Objects.Factories.*;
	import src.Objects.GameObject;
	import src.Objects.Actions.ActionButton;
	import src.Objects.Location;
	import src.Objects.Process.AttackSendProcess;
	import src.UI.Components.SimpleTooltip;
	import src.UI.Tooltips.TextTooltip;

	public class SendAttackButton extends ActionButton
	{
		private var tooltip: TextTooltip;

		public function SendAttackButton(parentObj: GameObject)
		{
			super(parentObj, "Send Attack");

			new SimpleTooltip(this, "Send attack to the selected structure.");

			addEventListener(MouseEvent.CLICK, onMouseClick);
		}

		public function onMouseClick(event: Event):void
		{
			if (isEnabled())
			{
				var process : AttackSendProcess = new AttackSendProcess(new Location(Location.CITY, parentObj.groupId, parentObj.objectId));
				process.execute();
			}
		}
	}

}



package src.UI.Sidebars.ObjectInfo.Buttons {
	import flash.events.Event;
	import flash.events.MouseEvent;
	import src.Objects.Factories.*;
	import src.Objects.GameObject;
	import src.Objects.Actions.ActionButton;
	import src.Objects.Location;
	import src.Objects.Process.ReinforcementSendProcess;
	import src.UI.Components.SimpleTooltip;
	import src.UI.Tooltips.TextTooltip;

	public class SendReinforcementButton extends ActionButton
	{
		private var tooltip: TextTooltip;

		public function SendReinforcementButton(parentObj: GameObject)
		{
			super(parentObj, "Send Reinforcement");

			new SimpleTooltip(this, "Send reinforcement to selected structure's city.");

			addEventListener(MouseEvent.CLICK, onMouseClick);
		}

		public function onMouseClick(event: Event):void
		{
			if (isEnabled())
			{
				var process : ReinforcementSendProcess = new ReinforcementSendProcess(new Location(Location.CITY, parentObj.groupId));
				process.execute();
			}
		}
	}

}


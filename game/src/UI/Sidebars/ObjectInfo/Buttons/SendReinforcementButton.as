
package src.UI.Sidebars.ObjectInfo.Buttons {
	import flash.events.Event;
	import flash.events.MouseEvent;
	import src.Global;
	import src.Objects.Factories.*;
	import src.Objects.Actions.ActionButton;
	import src.Objects.Location;
	import src.Objects.Process.ReinforcementSendProcess;
	import src.Objects.SimpleGameObject;
	import src.UI.Components.SimpleTooltip;
	import src.UI.Tooltips.TextTooltip;

	public class SendReinforcementButton extends ActionButton
	{
		private var tooltip: TextTooltip;
		private var location: Location;

		public function SendReinforcementButton(parentObj: SimpleGameObject, location: Location)
		{
			super(parentObj, "Send Reinforcement");

			this.location = location;
			new SimpleTooltip(this, "Send Reinforcement");

			addEventListener(MouseEvent.CLICK, onMouseClick);
		}

		public function onMouseClick(event: Event):void
		{
			if (isEnabled())
			{
				var process : ReinforcementSendProcess = new ReinforcementSendProcess(Global.gameContainer.selectedCity, location);
				process.execute();
			}
		}
	}

}


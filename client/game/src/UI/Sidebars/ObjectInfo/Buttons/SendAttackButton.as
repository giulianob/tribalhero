
package src.UI.Sidebars.ObjectInfo.Buttons {
    import flash.events.Event;
    import flash.events.MouseEvent;

    import src.Global;
    import src.FeathersUI.Controls.ActionButton;
    import src.Objects.Location;
    import src.Objects.Process.AttackSendProcess;
    import src.Objects.SimpleGameObject;
    import src.UI.Components.SimpleTooltip;
    import src.UI.Tooltips.TextTooltip;

    public class SendAttackButton extends ActionButton
	{
		private var tooltip: TextTooltip;
		private var location: Location;

		public function SendAttackButton(parentObj: SimpleGameObject, location: Location)
		{
			super(parentObj, "Send Attack");

//			new SimpleTooltip(this, "Send Attack");
			this.location = location;
			addEventListener(MouseEvent.CLICK, onMouseClick);
		}

		public function onMouseClick(event: Event):void
		{
			if (isEnabled)
			{
				var process : AttackSendProcess = new AttackSendProcess(Global.gameContainer.selectedCity);
				process.execute();
			}
		}
	}

}


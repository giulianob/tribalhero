
package src.UI.Sidebars.ObjectInfo.Buttons {
	import flash.display.SimpleButton;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import src.Global;
	import src.Map.City;
	import src.Objects.Factories.*;
	import src.Objects.*;
	import src.Objects.Actions.ActionButton;
	import src.UI.Components.SimpleTooltip;
	import src.UI.Cursors.*;
	import src.UI.Dialog.SendResourceDialog;

	public class SendResourcesButton extends ActionButton
	{
		public function SendResourcesButton(parentObj: SimpleGameObject)
		{
			super(parentObj, "Send Resources");

			new SimpleTooltip(this, "Send resources to other players.");

			addEventListener(MouseEvent.CLICK, onMouseClick);
		}

		public function onMouseClick(MouseEvent: Event):void
		{
			if (isEnabled())
			{						
				var picker: SendResourceDialog = new SendResourceDialog(parentObj as StructureObject, function(dlg: SendResourceDialog) : void {
					Global.mapComm.City.sendResources(dlg.amount(), parentObj.groupId, parentObj.objectId, dlg.cityName());
					dlg.getFrame().dispose();
				});
				
				picker.show();
			}
		}
	}

}


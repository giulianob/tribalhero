
package src.UI.Sidebars.ObjectInfo.Buttons {
    import flash.events.*;

    import src.*;
    import src.Objects.*;
    import src.Objects.Actions.*;
import src.UI.Components.SimpleTooltip;
import src.UI.Dialog.SendResourceDialog;
import src.UI.Tooltips.*;

    public class ResourceWithdrawButton extends ActionButton
	{
        public function ResourceWithdrawButton(parentObj: SimpleGameObject)
        {
            super(parentObj, "Withdraw Resources");

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


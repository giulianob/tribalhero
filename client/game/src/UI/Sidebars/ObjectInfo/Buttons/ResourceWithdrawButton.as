
package src.UI.Sidebars.ObjectInfo.Buttons {
    import flash.events.*;

    import src.*;
    import src.Objects.*;
    import src.Objects.Actions.*;
import src.UI.Components.SimpleTooltip;
import src.UI.Dialog.ResourceWithdrawDialog;
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
                var picker: ResourceWithdrawDialog = new ResourceWithdrawDialog(parentObj as StructureObject, function(dlg: ResourceWithdrawDialog) : void {
                    dlg.getFrame().dispose();
                });

                picker.show();
            }
        }
	}

}


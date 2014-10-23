/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.Objects.Actions {
    import src.FeathersUI.Controls.ActionButton;
    import src.Objects.Prototypes.StructurePrototype;
    import src.Objects.SimpleGameObject;
    import src.UI.Sidebars.ObjectInfo.Buttons.SendResourcesButton;

    public class SendResourcesAction extends Action implements IAction
	{
		public function SendResourcesAction()
		{
			super(Action.RESOURCE_SEND);
		}

		public function toString(): String
		{
			return "Sending Resources";
		}

		public function getButton(parentObj: SimpleGameObject, sender: StructurePrototype): ActionButton
		{
			return new SendResourcesButton(parentObj) as ActionButton;
		}

	}

}


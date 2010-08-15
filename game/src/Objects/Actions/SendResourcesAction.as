/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.Objects.Actions {
	import src.Objects.Actions.IAction;
	import src.Objects.GameObject;
	import src.Objects.Prototypes.StructurePrototype;
	import src.UI.Sidebars.ObjectInfo.Buttons.LaborMoveButton;
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

		public function getButton(parentObj: GameObject, sender: StructurePrototype): ActionButton
		{
			return new SendResourcesButton(parentObj) as ActionButton;
		}

	}

}


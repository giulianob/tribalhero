/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.Objects.Actions {
import src.Objects.Prototypes.StructurePrototype;
import src.Objects.SimpleGameObject;
import src.UI.Sidebars.ObjectInfo.Buttons.ResourceWithdrawButton;

public class ResourceWithdrawAction extends Action implements IAction
	{
		public function ResourceWithdrawAction()
		{
			super(Action.RESOURCES_WITHDRAW);
		}

		public function toString(): String
		{
			return "Withdraw resources";
		}

		public function getButton(parentObj: SimpleGameObject, sender: StructurePrototype): ActionButton
		{
			return new ResourceWithdrawButton(parentObj);
		}

	}

}


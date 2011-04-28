/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.Objects.Actions {
	import src.Objects.Actions.IAction;
	import src.Objects.GameObject;
	import src.Objects.Prototypes.StructurePrototype;
	import src.Objects.SimpleGameObject;
	import src.UI.Sidebars.ObjectInfo.Buttons.StructureUserDowngradeButton;

	public class StructureUserDowngradeAction extends Action implements IAction
	{
		public function StructureUserDowngradeAction()
		{
			super(Action.STRUCTURE_USERDOWNGRADE);
		}

		public function toString(): String
		{
			return "Downgrading structure";
		}

		public function getButton(parentObj: SimpleGameObject, sender: StructurePrototype): ActionButton
		{
			return new StructureUserDowngradeButton(parentObj);
		}

	}

}


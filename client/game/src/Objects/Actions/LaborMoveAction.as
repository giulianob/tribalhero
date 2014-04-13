/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.Objects.Actions {
    import src.Objects.Prototypes.StructurePrototype;
    import src.Objects.SimpleGameObject;
    import src.UI.Sidebars.ObjectInfo.Buttons.LaborMoveButton;

    public class LaborMoveAction extends Action implements IAction
	{
		public function LaborMoveAction()
		{
			super(Action.LABOR_MOVE);
		}

		public function toString(): String
		{
			return "Transferring Workers";
		}

		public function getButton(parentObj: SimpleGameObject, sender: StructurePrototype): ActionButton
		{
			return new LaborMoveButton(parentObj) as ActionButton;
		}

	}

}


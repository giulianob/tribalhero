/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.Objects.Actions {
    import src.Objects.Prototypes.StructurePrototype;
    import src.Objects.SimpleGameObject;
    import src.UI.Sidebars.ObjectInfo.Buttons.ForestCampRemoveButton;

    public class ForestCampRemoveAction extends Action implements IAction
	{
		public function ForestCampRemoveAction()
		{
			super(Action.FOREST_CAMP_REMOVE);
		}

		public function toString(): String
		{
			return "Taking camp down";
		}

		public function getButton(parentObj: SimpleGameObject, sender: StructurePrototype): ActionButton
		{
			return new ForestCampRemoveButton(parentObj) as ActionButton;
		}

	}

}


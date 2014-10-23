/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.Objects.Actions {
    import src.FeathersUI.Controls.ActionButton;
    import src.Objects.Prototypes.StructurePrototype;
    import src.Objects.SimpleGameObject;
    import src.UI.Sidebars.ObjectInfo.Buttons.DestroyRoadButton;

    public class DestroyRoadAction extends Action implements IAction
	{
		public function DestroyRoadAction()
		{
			super(Action.ROAD_DESTROY);
		}

		public function toString(): String
		{
			return "Destroying road";
		}

		public function getButton(parentObj: SimpleGameObject, sender: StructurePrototype): ActionButton
		{
			return new DestroyRoadButton(parentObj);
		}

	}

}


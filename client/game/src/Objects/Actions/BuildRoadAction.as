/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.Objects.Actions {
    import src.FeathersUI.Controls.ActionButton;
    import src.Objects.Prototypes.StructurePrototype;
    import src.Objects.SimpleGameObject;
    import src.UI.Sidebars.ObjectInfo.Buttons.BuildRoadButton;

    public class BuildRoadAction extends Action implements IAction
	{
		public function BuildRoadAction()
		{
			super(Action.ROAD_BUILD);
		}

		public function toString(): String
		{
			return "Building road";
		}

		public function getButton(parentObj: SimpleGameObject, sender: StructurePrototype): ActionButton
		{
			return new BuildRoadButton(parentObj);
		}

	}

}


/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.Objects.Actions {
    import src.FeathersUI.Controls.ActionButton;
    import src.Objects.Prototypes.StructurePrototype;
    import src.Objects.SimpleGameObject;
    import src.UI.Sidebars.ForestInfo.Buttons.ForestCampBuildButton;

    public class ForestCampBuildAction extends Action implements IAction
	{
		public var campType: int;
		
		public function ForestCampBuildAction(campType: int)
		{
			super(Action.FOREST_CAMP_BUILD);
			
			this.campType = campType;
		}

		public function toString(): String
		{
			return "Building Lumbermill Outpost";
		}

		public function getButton(parentObj: SimpleGameObject, sender: StructurePrototype): ActionButton
		{
			return new ForestCampBuildButton(parentObj) as ActionButton;
		}

	}

}


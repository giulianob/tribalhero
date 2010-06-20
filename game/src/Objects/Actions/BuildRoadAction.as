/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.Objects.Actions {
	import flash.display.SimpleButton;
	import flash.utils.getDefinitionByName;
	import src.Objects.Actions.IAction;
	import src.Objects.GameObject;
	import src.Objects.Prototypes.StructurePrototype;
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

		public function getButton(parentObj: GameObject, sender: StructurePrototype): ActionButton
		{
			return new BuildRoadButton(parentObj);
		}

	}

}


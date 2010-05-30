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

		public function getButton(parentObj: GameObject, sender: StructurePrototype): ActionButton
		{
			var objRef:Class = getDefinitionByName("DEFAULT_ROAD_DESTROY_BUTTON") as Class;

			var ui: SimpleButton = new objRef() as SimpleButton;

			if (ui == null)
			return null;

			return new DestroyRoadButton(ui, parentObj);
		}

	}

}


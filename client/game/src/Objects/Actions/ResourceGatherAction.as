/**
* ...
* @author Default
* @version 0.1
*/

package src.Objects.Actions {
    import src.FeathersUI.Controls.ActionButton;
    import src.Objects.Prototypes.StructurePrototype;
    import src.Objects.SimpleGameObject;
    import src.UI.Sidebars.ObjectInfo.Buttons.*;

    public class ResourceGatherAction extends Action implements IAction
	{
		public function ResourceGatherAction()
		{
			super(Action.RESOURCES_GATHER);
		}
		
		public function toString(): String
		{
			return "Harvesting";
		}
		
		public function getButton(parentObj: SimpleGameObject, sender: StructurePrototype): ActionButton
		{		
			return new ResourceGatherButton(parentObj) as ActionButton;
		}
		
	}
	
}

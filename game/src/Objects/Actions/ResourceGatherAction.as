/**
* ...
* @author Default
* @version 0.1
*/

package src.Objects.Actions {
	import flash.display.SimpleButton;
	import flash.utils.getDefinitionByName;
	import src.Constants;
	import src.Map.City;
	import src.Map.Map;
	import src.Objects.Actions.IAction;
	import src.Objects.Factories.StructureFactory;
	import src.Objects.GameObject;
	import src.Objects.IObject;
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
		
		public function getButton(parentObj: GameObject, sender: StructurePrototype): ActionButton
		{		
			return new ResourceGatherButton(parentObj) as ActionButton;
		}
		
	}
	
}

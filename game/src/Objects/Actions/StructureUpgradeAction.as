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
	import src.UI.Sidebars.ObjectInfo.Buttons.StructureUpgradeButton;
	
	public class StructureUpgradeAction extends Action implements IAction
	{
		public var type: int;
		
		public function StructureUpgradeAction(type: int)
		{
			super(Action.STRUCTURE_UPGRADE);
			this.type = type;
		}
		
		public function toString(): String
		{
			return "Upgrading Building";
		}
		
		public function getButton(parentObj: GameObject, sender: StructurePrototype): ActionButton
		{			
			return new StructureUpgradeButton(parentObj, sender) as ActionButton;
		}
		
	}
	
}

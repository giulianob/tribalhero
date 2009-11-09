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
	import src.Objects.GameObject;
	import src.Objects.IObject;
	import src.Objects.Prototypes.StructurePrototype;
	import src.Objects.SimpleGameObject;
	import src.UI.Sidebars.ObjectInfo.Buttons.LaborMoveButton;
	import src.UI.Sidebars.ObjectInfo.Buttons.MarketButton;
	
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
		
		public function getButton(parentObj: GameObject, sender: StructurePrototype): ActionButton
		{			
			var objRef: * = getDefinitionByName("DEFAULT_LABOR_MOVE_BUTTON") as Class;							
			
			var btn: SimpleButton = new objRef() as SimpleButton;
			
			if (btn == null)
				return null;
			
			return new LaborMoveButton(btn, parentObj) as ActionButton;
		}
		
	}
	
}

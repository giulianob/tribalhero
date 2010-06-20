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
	import src.UI.Sidebars.ObjectInfo.Buttons.MarketButton;
	
	public class MarketAction extends Action implements IAction
	{
		private var mode: String;
		
		public function MarketAction(mode: String)
		{			
			super(0);
			
			if (mode == "buy")
				actionType = Action.RESOURCE_BUY;
			else
				actionType = Action.RESOURCE_SELL;
				
			this.mode = mode;
		}
		
		public function toString(): String
		{									
			return "Trading Resources";
		}
		
		public function getButton(parentObj: GameObject, sender: StructurePrototype): ActionButton
		{						
			return new MarketButton(parentObj, mode) as ActionButton;
		}
		
	}
	
}

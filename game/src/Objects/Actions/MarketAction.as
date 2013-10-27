/**
* ...
* @author Default
* @version 0.1
*/

package src.Objects.Actions {
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
		
		public function getButton(parentObj: SimpleGameObject, sender: StructurePrototype): ActionButton
		{						
			return new MarketButton(parentObj, mode) as ActionButton;
		}
		
	}
	
}

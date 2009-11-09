/**
* ...
* @author Default
* @version 0.1
*/

package src.Objects.Actions {
	import flash.display.SimpleButton;
	import flash.utils.getDefinitionByName;
	import src.Constants;
	import src.Global;
	import src.Map.City;
	import src.Map.Map;
	import src.Objects.Actions.IAction;
	import src.Objects.Factories.UnitFactory;
	import src.Objects.GameObject;
	import src.Objects.IObject;
	import src.Objects.Prototypes.UnitPrototype;
	import src.Objects.Prototypes.StructurePrototype;
	import src.Objects.SimpleGameObject;
	import src.UI.Sidebars.ObjectInfo.Buttons.TrainButton;
	
	public class TrainAction extends Action implements IAction
	{	
		public var type: int;		
		
		public function TrainAction(type: int)
		{
			super(Action.UNIT_TRAIN);
			this.type = type;
		}
		
		public function toString(): String
		{												
			var unitPrototype: UnitPrototype = UnitFactory.getPrototype(type, 1);

			return "Training " + unitPrototype.getName();			
		}
		
		public function getButton(parentObj: GameObject, sender: StructurePrototype): ActionButton
		{			
			var unitPrototype: UnitPrototype = UnitFactory.getPrototype(type, Global.map.cities.getTemplateLevel(parentObj.cityId, type));
			var objRef:Class;						
			
			if (unitPrototype != null)
			{							
				try
				{
					objRef = getDefinitionByName(unitPrototype.spriteClass + "_TRAIN_BUTTON") as Class;
				}
				catch (error: Error)
				{
					trace("Missing button sprite class: " + unitPrototype.spriteClass + "_TRAIN_BUTTON. Loading default");
					objRef = getDefinitionByName("DEFAULT_TRAIN_BUTTON") as Class;
				}
			}
			else
			{
				trace("Missing prototype class when creating button type: " + type + " level " + Global.map.cities.getTemplateLevel(parentObj.cityId, type) + ". Loading default");
				objRef = getDefinitionByName("DEFAULT_TRAIN_BUTTON") as Class;				
			}
			
			var btn: SimpleButton = new objRef() as SimpleButton;
						
			if (btn == null)
				return null;
			
			return new TrainButton(btn, parentObj, unitPrototype) as ActionButton;
		}
		
	}
	
}

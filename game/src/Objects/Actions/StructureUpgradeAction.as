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
			var structPrototype: StructurePrototype = sender;
			var objRef:Class;
			
			if (structPrototype != null)
			{							
				try
				{
					objRef = getDefinitionByName(structPrototype.spriteClass + "_UPGRADE_BUTTON") as Class;
				}
				catch (error: Error)
				{
					trace("Missing button sprite class: " + structPrototype.spriteClass + "_UPGRADE_BUTTON. Loading default");
					objRef = getDefinitionByName("DEFAULT_STRUCTURE_UPGRADE_BUTTON") as Class;
				}
			}
			else
			{
				trace("Missing prototype class when creating button type: " + structPrototype.type + " level " + structPrototype.level + ". Loading default");
				objRef = getDefinitionByName("DEFAULT_STRUCTURE_UPGRADE_BUTTON") as Class;				
			}
			
			var btn: SimpleButton = new objRef() as SimpleButton;
						
			if (btn == null)
				return null;
			
			return new StructureUpgradeButton(btn, parentObj, structPrototype) as ActionButton;
		}
		
	}
	
}

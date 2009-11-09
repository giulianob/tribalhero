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
	import src.UI.Sidebars.ObjectInfo.Buttons.StructureChangeButton;
	
	public class StructureChangeAction extends Action implements IAction
	{
		public var type: int;
		public var level: int;
		
		public function StructureChangeAction(type: int, level: int)
		{
			super(Action.STRUCTURE_CHANGE);
			this.type = type;
			this.level = level;
		}
		
		public function toString(): String
		{
			return "Converting to " + StructureFactory.getPrototype(type, 1).getName();
		}
		
		public function getButton(parentObj: GameObject, sender: StructurePrototype): ActionButton
		{
			var structPrototype: StructurePrototype = StructureFactory.getPrototype(type, level);
			var objRef:Class;
			
			if (structPrototype != null)
			{							
				try
				{
					objRef = getDefinitionByName(structPrototype.spriteClass + "_UPGRADE_BUTTON") as Class;
				}
				catch (error: Error)
				{
					trace("Missing button sprite class: " + structPrototype.spriteClass + "_CHANGE_BUTTON. Loading default");
					objRef = getDefinitionByName("DEFAULT_CHANGE_BUTTON") as Class;
				}
			}
			else
			{
				trace("Missing prototype class when creating button type: " + structPrototype.type + " level " + structPrototype.level + " for ChangeAction. Loading default");
				objRef = getDefinitionByName("DEFAULT_CHANGE_BUTTON") as Class;
			}
			
			var btn: SimpleButton = new objRef() as SimpleButton;
			
			if (btn == null)
				return null;
			
			return new StructureChangeButton(btn, parentObj, sender, structPrototype) as ActionButton;
		}
		
	}
	
}

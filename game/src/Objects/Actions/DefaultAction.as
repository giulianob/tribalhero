package src.Objects.Actions 
{
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
	import src.UI.Sidebars.ObjectInfo.Buttons.BuildButton;
	import src.UI.Sidebars.ObjectInfo.Buttons.DefaultActionButton;
	
	public class DefaultAction extends Action implements IAction
	{
		public var type: int;
		public var level: int;
		public var name:String;
		
		public function DefaultAction(type: int, name: String)
		{
			super(0);
			this.type = type;
			this.name = name;
			level = 1;
		}
		
		public function toString(): String
		{
			return "Default Action " + type;
		}
		
		public function getButton(parentObj: GameObject, sender: StructurePrototype): ActionButton
		{
			var structPrototype: StructurePrototype = StructureFactory.getPrototype(type, level);
			var objRef:Class;
			
			if (structPrototype != null)
			{							
				try
				{
					objRef = getDefinitionByName(structPrototype.spriteClass + "_BUILD_BUTTON") as Class;
				}
				catch (error: Error)
				{
					trace("Missing button sprite class: " + structPrototype.spriteClass + "_BUILD_BUTTON. Loading default");
					objRef = getDefinitionByName("DEFAULT_ACTION_BUTTON") as Class;
				}
			}
			else
			{
				trace("Missing prototype class when creating button type: " + type + " level " + level + ". Loading default");
				objRef = getDefinitionByName("DEFAULT_ACTION_BUTTON") as Class;				
			}
			
			var btn: SimpleButton = new objRef() as SimpleButton;
						
			if (btn == null)
				return null;
						
			return new DefaultActionButton(btn, parentObj, structPrototype, name) as ActionButton;
		}
		
	}
	

	
}
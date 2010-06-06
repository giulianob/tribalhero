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
	import src.UI.Sidebars.ObjectInfo.Buttons.BuildButton;
	
	public class BuildAction extends Action implements IAction
	{	
		public var type: int;
		public var tilerequirement: String;
		public var level: int = 1;
		
		public function BuildAction(type: int, tilerequirement: String)
		{
			super(Action.STRUCTURE_BUILD);
			this.type = type;			
			this.tilerequirement = tilerequirement;
		}
		
		public function toString(): String
		{			
			var structPrototype: StructurePrototype = StructureFactory.getPrototype(type, level);
			if (structPrototype == null)
			{
				return "Building " + type;
			}
			else
			{
				return "Building " + structPrototype.getName();
			}
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
					objRef = getDefinitionByName("DEFAULT_BUILD_BUTTON") as Class;
				}
			}
			else
			{
				trace("Missing prototype class when creating button type: " + type + " level " + level + ". Loading default");
				objRef = getDefinitionByName("DEFAULT_BUILD_BUTTON") as Class;				
			}
			
			var ui: SimpleButton = new objRef() as SimpleButton;
						
			if (ui == null)
				return null;
			
			return new BuildButton(ui, parentObj, structPrototype);
		}
		
	}
	
}

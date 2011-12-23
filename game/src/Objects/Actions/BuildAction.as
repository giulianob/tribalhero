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
	import src.Objects.Prototypes.StructurePrototype;
	import src.Objects.SimpleGameObject;
	import src.UI.Sidebars.ObjectInfo.Buttons.BuildButton;
	
	public class BuildAction extends Action implements IAction
	{	
		public var type: int;
		public var tilerequirement: String;
		public var level: int = 1;
		
		public function BuildAction(type: int, tilerequirement: String, level: int)
		{
			super(Action.STRUCTURE_BUILD);
			this.type = type;			
			this.tilerequirement = tilerequirement;
			this.level = level;
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
		
		public function getButton(parentObj: SimpleGameObject, sender: StructurePrototype): ActionButton
		{		
			return new BuildButton(parentObj, StructureFactory.getPrototype(type, level));
		}
		
	}
	
}

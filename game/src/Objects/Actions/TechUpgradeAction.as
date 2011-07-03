/**
* ...
* @author Default
* @version 0.1
*/

package src.Objects.Actions {
	import flash.display.SimpleButton;
	import src.Constants;
	import src.Map.City;
	import src.Map.Map;
	import src.Objects.Actions.IAction;
	import src.Objects.Factories.TechnologyFactory;
	import src.Objects.GameObject;
	import src.Objects.Prototypes.StructurePrototype;
	import src.Objects.Prototypes.TechnologyPrototype;
	import src.Objects.SimpleGameObject;
	import src.Objects.StructureObject;
	import src.Objects.TechnologyStats;
	import src.UI.Sidebars.ObjectInfo.Buttons.BuildButton;

	public class TechUpgradeAction extends Action implements IAction
	{
		public var techtype: int;				
		public var maxlevel: int;
		
		public function TechUpgradeAction(techtype: int, maxlevel: int)
		{
			super(Action.TECHNOLOGY_UPGRADE);
			this.techtype = techtype;
			this.maxlevel = maxlevel;	
		}
		
		public function toString(): String
		{				
			var tech: TechnologyPrototype = TechnologyFactory.getPrototype(techtype, 1); //we assume level 1 because all techs have the same name
				
			if (!tech)
				return "Researching " + techtype;
			
			return "Researching " + tech.getName();
		}
		
		public function getButton(parentObj: SimpleGameObject, structPrototype: StructurePrototype): ActionButton
		{
			return null;
		}
	}
	
}

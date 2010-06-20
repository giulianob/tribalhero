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
	import src.Objects.Prototypes.StructurePrototype;
	import src.Objects.Prototypes.UnitPrototype;
	import src.Objects.SimpleGameObject;
	import src.Objects.Troop.*;
	import src.UI.Sidebars.ObjectInfo.Buttons.UnitUpgradeButton;
	
	public class UnitUpgradeAction extends Action implements IAction
	{
		public var type: int;
		public var maxlevel: int;
		
		public function UnitUpgradeAction(type: int, maxlevel: int)
		{
			super(Action.UNIT_UPGRADE);
			this.type = type;
			this.maxlevel = maxlevel;
		}
		
		public function toString(): String
		{		
			return "Upgrading " + UnitFactory.getPrototype(type, 1).getName(); //we assume level 1 here because all units have the same name
		}
		
		public function getButton(parentObj: GameObject, sender: StructurePrototype): ActionButton
		{
			var city: City = Global.map.cities.get(parentObj.cityId);
			var template: UnitTemplate = city.template.get(type);			
			var unitPrototype: UnitPrototype = UnitFactory.getPrototype(type, template?template.level:1);
			
			return new UnitUpgradeButton(parentObj, unitPrototype) as ActionButton;
		}
		
	}
	
}

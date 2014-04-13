/**
* ...
* @author Default
* @version 0.1
*/

package src.Objects.Actions {
    import src.Global;
    import src.Objects.Factories.UnitFactory;
    import src.Objects.Prototypes.StructurePrototype;
    import src.Objects.Prototypes.UnitPrototype;
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
		
		public function getButton(parentObj: SimpleGameObject, sender: StructurePrototype): ActionButton
		{			
			var unitPrototype: UnitPrototype = UnitFactory.getPrototype(type, Global.map.cities.getTemplateLevel(parentObj.groupId, type));
			
			return new TrainButton(parentObj, unitPrototype) as ActionButton;
		}
		
	}
	
}

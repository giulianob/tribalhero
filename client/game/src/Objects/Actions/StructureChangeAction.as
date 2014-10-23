/**
* ...
* @author Default
* @version 0.1
*/

package src.Objects.Actions {
    import src.FeathersUI.Controls.ActionButton;
    import src.Objects.Factories.StructureFactory;
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
			return "Converting to " + StructureFactory.getPrototype(type, level).getName();
		}
		
		public function getButton(parentObj: SimpleGameObject, sender: StructurePrototype): ActionButton
		{
			var structPrototype: StructurePrototype = StructureFactory.getPrototype(type, level);
			
			return new StructureChangeButton(parentObj, sender, structPrototype) as ActionButton;
		}
		
	}
	
}

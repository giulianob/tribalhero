/**
* ...
* @author Default
* @version 0.1
*/

package src.Objects.Actions {
    import src.Objects.Prototypes.StructurePrototype;
    import src.Objects.SimpleGameObject;
    import src.UI.Sidebars.ObjectInfo.Buttons.*;

    public class StructureSelfDestroyAction extends Action implements IAction
	{
		public function StructureSelfDestroyAction()
		{
			super(Action.STRUCTURE_SELF_DESTROY);
		}
		
		public function toString(): String
		{
			return "Removing";
		}
		
		public function getButton(parentObj: SimpleGameObject, sender: StructurePrototype): ActionButton
		{		
			return new StructureSelfDestroyButton(parentObj) as ActionButton;
		}
		
	}
	
}

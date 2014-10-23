/**
* ...
* @author Default
* @version 0.1
*/

package src.Objects.Actions {
    import src.FeathersUI.Controls.ActionButton;
    import src.Objects.Prototypes.StructurePrototype;
    import src.Objects.SimpleGameObject;
    import src.UI.Sidebars.ObjectInfo.Buttons.StructureUpgradeButton;

    public class StructureUpgradeAction extends Action implements IAction
	{
	
		public function StructureUpgradeAction()
		{
			super(Action.STRUCTURE_UPGRADE);
		}
		
		public function toString(): String
		{
			return "Upgrading Building";
		}
		
		public function getButton(parentObj: SimpleGameObject, sender: StructurePrototype): ActionButton
		{			
			return new StructureUpgradeButton(parentObj, sender) as ActionButton;
		}
		
	}
	
}

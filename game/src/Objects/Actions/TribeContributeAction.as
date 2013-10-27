/**
* ...
* @author Default
* @version 0.1
*/

package src.Objects.Actions {
    import src.Objects.Prototypes.StructurePrototype;
    import src.Objects.SimpleGameObject;
    import src.UI.Sidebars.ObjectInfo.Buttons.TribeContributeButton;

    public class TribeContributeAction extends Action implements IAction
	{
		public function TribeContributeAction()
		{			
			super(Action.TRIBE_CONTRIBUTE);
		}
		
		public function toString(): String
		{									
			return "Contribute Tribe Resources";
		}
		
		public function getButton(parentObj: SimpleGameObject, sender: StructurePrototype): ActionButton
		{						
			return new TribeContributeButton(parentObj) as ActionButton;
		}
		
	}
	
}

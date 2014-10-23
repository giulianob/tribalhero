/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.Objects.Actions {
    import src.FeathersUI.Controls.ActionButton;
    import src.Objects.Prototypes.StructurePrototype;
    import src.Objects.SimpleGameObject;
    import src.UI.Sidebars.ObjectInfo.Buttons.DefaultActionButton;

    public class DefaultAction extends Action implements IAction
	{
		private var command:int;
		
		public function DefaultAction(_command:int)
		{
			super(Action.DEFAULT_ACTION);
			this.command = _command;
		}

		public function toString(): String
		{
			return "Default";
		}

		public function getButton(parentObj: SimpleGameObject, sender: StructurePrototype): ActionButton
		{
			return new DefaultActionButton(parentObj,command);
		}

	}

}


/**
* ...
* @author Default
* @version 0.1
*/

package src.Objects.Actions {
    import src.FeathersUI.Controls.ActionButton;
    import src.Objects.Prototypes.StructurePrototype;
    import src.Objects.SimpleGameObject;

    public interface IAction {
		function getButton(parentObj: SimpleGameObject, structPrototype: StructurePrototype): ActionButton;		
		function toString(): String;
	}
	
}

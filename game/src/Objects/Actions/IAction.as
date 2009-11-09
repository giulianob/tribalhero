/**
* ...
* @author Default
* @version 0.1
*/

package src.Objects.Actions {
	import src.Objects.Actions.ActionButton;
	import src.Objects.GameObject;
	import src.Objects.IObject;
	import src.Objects.Prototypes.StructurePrototype;
	import src.Objects.SimpleGameObject;
	
	public interface IAction {
		function getButton(parentObj: GameObject, structPrototype: StructurePrototype): ActionButton;		
		function toString(): String;
	}
	
}

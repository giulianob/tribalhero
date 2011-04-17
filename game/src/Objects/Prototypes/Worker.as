/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.Objects.Prototypes {
	import src.Objects.Actions.Action;
	import src.Objects.Actions.ActionButton;
	import src.Objects.Actions.IAction;
	import src.Objects.Actions.TechUpgradeAction;
	import src.Objects.GameObject;
	import src.Objects.SimpleGameObject;

	public class Worker {

		public var type: int;
		public var maxCount: int;

		private var actions: Array = new Array();
		private var techUpgradeActions: Array = new Array();

		public function Worker() {
		}

		public function addAction(action: IAction): void
		{
			actions.push(action);
		}

		public function addTechUpgradeAction(action: TechUpgradeAction):void
		{
			techUpgradeActions.push(action);
		}

		public function getAction(index: int): *
		{
			for each(var action: Action in actions) {
				if (action.index == index) return action;
			}
			
			return null;
		}

		public function getTechUpgradeActions(): Array
		{
			var upgradeActions: Array = new Array();
			for each (var technology: TechUpgradeAction in techUpgradeActions)
			{
				upgradeActions.push(technology);
			}

			return upgradeActions;
		}

		public function getButtons(parentObj: SimpleGameObject, structPrototype: StructurePrototype): Array
		{
			var ret: Array = new Array();

			for each (var action: IAction in actions)
			{
				var btn: ActionButton = action.getButton(parentObj, structPrototype);

				if (btn == null)
				continue;

				btn.parentAction = action as Action;

				ret.push(btn);
			}

			return ret;
		}

		public static function sortOnType(a:Worker, b:Worker):Number {
			var aType:Number = a.type;
			var bType:Number = b.type;

			if(aType > bType) {
				return 1;
			} else if(aType < bType) {
				return -1;
			} else  {
				return 0;
			}
		}

		public static function compare(a: Worker, value: int): int
		{
			return a.type - value;
		}
	}

}


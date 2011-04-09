/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.Objects.Actions {
	import flash.events.MouseEvent;
	import flash.filters.*;
	import org.aswing.AsWingConstants;
	import org.aswing.Icon;
	import org.aswing.JButton;
	import src.Global;
	import src.Map.City;
	import src.Objects.GameObject;
	import src.UI.LookAndFeel.GameLookAndFeel;

	public class ActionButton extends JButton {
		protected var parentObj: GameObject;

		public var parentAction: Action = new Action();
		public var currentCount: int;

		public function ActionButton(parentObj: GameObject, buttonText: String, icon: Icon = null)
		{
			super(buttonText, icon);

			GameLookAndFeel.changeClass(this, "Button.action");

			setPreferredHeight(20);
			setHorizontalAlignment(AsWingConstants.LEFT);
			this.parentObj = parentObj;

			addEventListener(MouseEvent.CLICK, eventMouseClick);
			addEventListener(MouseEvent.MOUSE_DOWN, eventMouseClick);
		}

		public function eventMouseClick(e: MouseEvent):void
		{
			e.stopPropagation();
		}

		public function enable():void {
			setEnabled(true);
			//filters = new Array();
		}

		public function disable():void {
			setEnabled(false);
			mouseEnabled = true;
			//filters = [new DropShadowFilter(width, 0, 0, 0.4, 0, 0, 1, 1, true, false, false)];
		}

		public function alwaysEnabled(): Boolean 
		{
			return false;
		}
		
		public function validateButton(): Boolean
		{
			return true;
		}

		public function countCurrentActions(): Boolean
		{
			if (!parentAction) return true;

			currentCount = 0;

			var city: City = Global.map.cities.get(parentObj.cityId);
			if (city == null) return true;

			var actions: Array = city.currentActions.getObjectActions(parentObj.objectId);

			for each (var currentAction: * in actions)
			{
				var action: CurrentAction;

				if (currentAction is CurrentActionReference) continue;
				else action = currentAction;

				if (!(action is CurrentActiveAction)) continue;

				if (action.endTime -  Global.map.getServerTime() <= 0) continue;

				if ((action as CurrentActiveAction).index == parentAction.index) currentCount++;
			}

			return currentCount < parentAction.maxCount;
		}
	}
}


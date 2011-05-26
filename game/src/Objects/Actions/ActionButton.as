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
	import src.Objects.SimpleGameObject;
	import src.UI.LookAndFeel.GameLookAndFeel;

	public class ActionButton extends JButton {
		protected var parentObj: SimpleGameObject;

		public var parentAction: Action = new Action();
		public var currentCount: int;

		public function ActionButton(parentObj: SimpleGameObject, buttonText: String, icon: Icon = null)
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
		}

		public function disable():void {
			setEnabled(false);
			mouseEnabled = true;
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
			var gameObj: GameObject = parentObj as GameObject;
			
			if (!parentAction || !gameObj) 
				return true;						

			var city: City = Global.map.cities.get(gameObj.cityId);
			
			if (city == null) 
				return true;

			var actions: Array = city.currentActions.getObjectActions(gameObj.objectId);

			currentCount = 0;
			for each (var currentAction: * in actions)
			{			
				if (currentAction is CurrentActionReference) 
					continue;
				
				if (!(currentAction is CurrentActiveAction)) 
					continue;

				if (currentAction.endTime - Global.map.getServerTime() <= 0) 
					continue;

				if (currentAction.index == parentAction.index)
					continue;
					
				currentCount++;
			}

			return currentCount < parentAction.maxCount;
		}
	}
}


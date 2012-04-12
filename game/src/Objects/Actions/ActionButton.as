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
		public var parentObj: SimpleGameObject;

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
	}
}


/**
* ...
* @author Default
* @version 0.1
*/

package src.Objects.Actions {
	import adobe.utils.CustomActions;
	import flash.display.MovieClip;
	import flash.display.SimpleButton;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import flash.filters.*;
	import src.Global;
	import src.Map.City;
	import src.Map.CityObject;
	import src.Map.Map;
	import src.Objects.GameObject;
	import src.Objects.SimpleGameObject;
	import src.Objects.StructureObject;
	import src.Objects.Prototypes.Worker;
	
	public class ActionButton extends MovieClip {
		protected var ui: SimpleButton;		
		protected var parentObj: GameObject;
		
		public var parentAction: Action;
		public var currentCount: int;
		
		public function ActionButton(button: SimpleButton, parentObj: GameObject)
		{
			this.parentObj = parentObj;
			
			this.ui = button;
			addChild(button);						
			
			ui.addEventListener(MouseEvent.CLICK, eventMouseClick);
			ui.addEventListener(MouseEvent.MOUSE_DOWN, eventMouseClick);
		}
		
		public function eventMouseClick(e: MouseEvent):void
		{
			e.stopPropagation();
		}
				
		public function enable():void {
			enabled = true;
			ui.enabled = true;
			ui.filters = new Array();
		}
		
		public function disable():void {
			enabled = false;
			ui.enabled = false;								
			ui.filters = [new DropShadowFilter(width, 0, 0, 0.4, 0, 0, 1, 1, true, false, false)];
		}
		
		public function validateButton(): Boolean
		{
			return true;
		}
		
		public function countCurrentActions(): Boolean
		{
			if (!parentAction)
				return true;
			
			currentCount = 0;
			
			var city: City = Global.map.cities.get(parentObj.cityId);
			if (city == null) return true;
			
			var actions: Array = city.currentActions.getObjectActions(parentObj.objectId);
			
			for each (var currentAction: * in actions)
			{		
				var action: CurrentAction;
				
				if (currentAction is CurrentActionReference)		
					continue;				
				else
					action = currentAction;
					
				if (!(action is CurrentActiveAction))
					continue;
					
				if (action.endTime -  Global.map.getServerTime() <= 0)
					continue;
					
				if ((action as CurrentActiveAction).index == parentAction.index)
				{
					currentCount++;
				}
			}

			return currentCount < parentAction.maxCount;
		}		
	}
}

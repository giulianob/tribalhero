/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.Objects {
	import flash.display.DisplayObject;
	import flash.events.Event;
	import flash.geom.Point;
	import flash.text.TextField;
	import src.Constants;
	import src.Global;
	import src.Map.City;
	import src.Map.CityObject;
	import src.Map.Map;
	import src.Objects.States.GameObjectState;
	import src.UI.Components.GroundCircle;
	import src.Util.BinaryList.*;

	public class SimpleGameObject extends SimpleObject {
		
		public static var OBJECT_UPDATE: String = "OBJECT_UPDATE";
		
		public static var STATE_NORMAL: int = 0;
		public static var STATE_BATTLE: int = 1;
		public static var STATE_MOVING: int = 2;		
		
		public var type: int;		
		public var objectId: int;
		public var groupId: int;	
		
		private var icon: DisplayObject;		
		
		public function SimpleGameObject(type: int, state: GameObjectState, objX: int, objY: int, groupId: int, objectId: int)
		{
			this.type = type;
			this.groupId = groupId;
			this.objectId = objectId;
			this.objX = objX;
			this.objY = objY;
			
			State = state;
			
			mouseEnabled = false;
			addEventListener(OBJECT_UPDATE, onObjectUpdate);
		}		

		protected function onObjectUpdate(e: Event): void {			
		}		

		private var state: GameObjectState = new GameObjectState();
		public function get State(): GameObjectState
		{
			return state;
		}

		public function set State(value: GameObjectState):void
		{
			state = value;
			setIcon(value.getStateIcon());
		}

		private function setIcon(icon: DisplayObject):void
		{
			if (this.icon)			
				removeChild(this.icon);

			this.icon = icon;

			if (icon)
				addChild(icon);
		}

		public static function sortOnId(a:SimpleGameObject, b:SimpleGameObject):Number
		{
			var aId:Number = a.objectId;
			var bId:Number = b.objectId;

			if(aId > bId)
				return 1;
			else if(aId < bId)
				return -1;
			else 
				return 0;
		}

		public static function sortOnGroupIdAndObjId(a:SimpleGameObject, b:SimpleGameObject):Number {
			var aGroupId:Number = a.groupId;
			var bGroupId:Number = b.groupId;

			var aObjId:Number = a.objectId;
			var bObjId:Number = b.objectId;

			if (aGroupId > bGroupId)
				return 1;
			else if (aGroupId < bGroupId)
				return -1;
			else if (aObjId > bObjId)
				return 1;
			else if (aObjId < bObjId)
				return -1;
			else
				return 0;
		}
		
		public function equalById(obj: SimpleObject) : Boolean
		{
			var gameObj: SimpleGameObject = obj as SimpleGameObject;
			
			if (gameObj == null)
				return false;
				
			return groupId == gameObj.groupId && objectId == gameObj.objectId;
		}

		public function copy(obj: SimpleGameObject):void
		{			
		}
		
		public function equalsOnMap(obj: SimpleObject):Boolean
		{			
			var gameObj: SimpleGameObject = obj as SimpleGameObject;
			if (gameObj == null)
				return false;
			
			return type == gameObj.type;
		}		

		public static function compareGroupIdAndObjId(a: SimpleGameObject, value: Array):int
		{
			var groupDelta: int = a.groupId - value[0];
			var idDelta: int = a.objectId - value[1];

			if (groupDelta != 0)
				return groupDelta;
			else if (idDelta != 0)
				return idDelta;
			else
				return 0;
		}

		public static function compareObjId(a: SimpleGameObject, value: int):int
		{
			return a.objectId - value;
		}
	}

}


/**
* ...
* @author Default
* @version 0.1
*/

package src.Objects {
	import flash.display.MovieClip;
	import flash.events.EventDispatcher;
	import flash.events.MouseEvent;
	import flash.geom.Point;
	import flash.events.Event;
	import flash.text.TextField;
	import flash.text.TextFieldAutoSize;
	import flash.text.TextFieldType;
	import src.Constants;
	import src.Map.City;
	import src.Map.Map;
	import src.Map.MapUtil;
	import src.Objects.Actions.CurrentActionManager;
	import src.Objects.Prototypes.EffectPrototype;
	import src.Objects.Prototypes.TechnologyPrototype;
	import src.Objects.Actions.CurrentAction;	
	import src.UI.Components.GroundCircle;
	import src.Util.BinaryList;
	import src.Global;
	
	public class SimpleGameObject extends SimpleObject implements IObject {
		
		public static var OBJECT_UPDATE: String = "OBJECT_UPDATE";
		
		public static var STATE_BATTLE: int = 1;
		
		public var level: int;
		public var type: int;		
		public var playerId: int;
		public var objectId: int;
		public var cityId: int;		
		public var battleCityId: int;				
		public var state: int;				
		public var hp: int;				
		
		private var usernameLabel: TextField;		
		private var icon: MovieClip;	
		
		private var radiusVisible: Boolean = false;
		private var radius: int = 0;
		private var circle: GroundCircle;		
		
		public var actionReferences: CurrentActionManager = new CurrentActionManager();							
		public var wall: WallManager;		
		
		public function SimpleGameObject() 
		{
			mouseEnabled = false;
			addEventListener(OBJECT_UPDATE, onObjectUpdate);
		}
		
		public function init(map: Map, playerId: int, cityId: int, objectId: int, type: int):void
		{			
			this.type = type;
			this.playerId = playerId;
			this.objectId = objectId;
			this.cityId = cityId;
			
			wall = new WallManager(map, this);
		}
		
		public function getCityId(): int
		{
			return cityId;
		}
		
		public function getLevel(): int
		{
			return level;
		}
		
		public function getType(): int
		{
			return type;
		}
		
		public function dispose():void {
			if (wall) wall.clear();
			if (circle) hideRadius();
		}
		
		private function onObjectUpdate(e: Event): void {
			moveRadius();
		}
		
		public function showRadius():void {
			radiusVisible = true;
			moveRadius();
		}
		
		public function hideRadius():void {
			radiusVisible = false;			
			moveRadius();		
		}
		
		private function moveRadius():void
		{
			if (radius == 0 || !radiusVisible)
			{
				if (circle)
				{
					Global.map.objContainer.removeObject(circle, ObjectContainer.LOWER);
					circle = null;
				}
					
				return;
			}
			
			if (!circle)
			{							
				circle = new GroundCircle(radius);
				circle.alpha = 0.6;
			}			
			else
				Global.map.objContainer.removeObject(circle, ObjectContainer.LOWER);			
			
			circle.setX(getX());
			circle.setY(getY());
			
			circle.moveWithCamera(Global.map.gameContainer.camera);
			
			Global.map.objContainer.addObject(circle, ObjectContainer.LOWER);
		}		
		
		public function get Radius(): int 
		{
			return radius;
		}
		
		public function set Radius(value: int):void
		{
			radius = value;
			moveRadius();
		}
		
		public function get State(): int 
		{
			return state;
		}
		
		public function set State(value: int):void
		{
			state = value;
			
			switch(state)
			{
				case STATE_BATTLE:
					setIcon(new ICON_BATTLE());
					break;
				default:
					setIcon(null);
					break;
			}
		}	
		
		private function setIcon(icon: MovieClip):void
		{
			if (this.icon)
			{
				removeChild(this.icon);
				this.icon = null;				
			}
						
			this.icon = icon;
			
			if (icon)
				addChild(icon);
		}
		
		public function setProperties(level: int, hpPercent: int, objX: int, objY : int):void
		{			
			this.level = level;
			this.objX = objX;
			this.objY = objY;
		}
		
		public static function sortOnId(a:SimpleGameObject, b:SimpleGameObject):Number 
		{
			var aId:Number = a.objectId;
			var bId:Number = b.objectId;

			if(aId > bId) {
				return 1;
			} else if(aId < bId) {
				return -1;
			} else  {
				return 0;
			}
		}
		
		public static function sortOnCityIdAndObjId(a:SimpleGameObject, b:SimpleGameObject):Number {
			var aCityId:Number = a.cityId;
			var bCityId:Number = b.cityId;

			var aObjId:Number = a.objectId;
			var bObjId:Number = b.objectId;
			
			if (aCityId > bCityId)
				return 1;
			else if (aCityId < bCityId)
				return -1;
			else if (aObjId > bObjId)
				return 1;
			else if (aObjId < bObjId)
				return -1;
			else
				return 0;
		}
		
		public function showUsername(username: String):void
		{
			if (usernameLabel)
			{	
				removeChild(usernameLabel);
				usernameLabel = null;
			}			
			
			usernameLabel = new TextField();
			usernameLabel.autoSize = TextFieldAutoSize.LEFT;
			usernameLabel.type = TextFieldType.DYNAMIC;			
			usernameLabel.text = username;
			usernameLabel.mouseEnabled = false;
			usernameLabel.tabEnabled = false;
			usernameLabel.selectable = false;
			usernameLabel.x = Math.round(Constants.tileW/2.0) - Math.round(usernameLabel.width/2.0);
			usernameLabel.y = -25 - usernameLabel.height;
			addChild(usernameLabel);
		}		
		
		public function copy(obj: SimpleGameObject):void
		{
			obj.actionReferences = actionReferences;
		}
		
		public function distance(x_1: int, y_1: int): int
		{
            var offset: int = 0;
			
			var objPos: Point = new Point();
			
			objPos.x = getX();
			objPos.y = getY();
			
            if (objPos.y % 2 == 1 && y_1 % 2 == 0 && x_1 <= objPos.x) offset = 1;
            if (objPos.y % 2 == 0 && y_1 % 2 == 1 && x_1 >= objPos.x) offset = 1;
			
            return ((x_1 > objPos.x ? x_1 - objPos.x : objPos.x - x_1) + (y_1 > objPos.y ? y_1 - objPos.y : objPos.y - y_1) / 2 + offset);		
        }
		
		public static function compareCityIdAndObjId(a: SimpleGameObject, value: Array):int
		{
			var cityDelta: int = a.cityId - value[0];
			var idDelta: int = a.objectId - value[1];
			
			if (cityDelta != 0)
				return cityDelta;
				
			if (idDelta != 0)
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

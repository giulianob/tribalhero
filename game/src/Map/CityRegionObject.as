package src.Map
{
	import flash.display.*;
	import flash.events.MouseEvent;
	import flash.filters.GlowFilter;
	import flash.geom.ColorTransform;
	import flash.geom.Point;
	import src.Global;
	import src.Objects.Factories.ObjectFactory;
	import src.Objects.IScrollableObject;
	import src.UI.Tooltips.MinimapInfoTooltip;
	import src.Util.BinaryList.*;
	import src.Util.Util;
	import src.Constants;
	import src.Map.Map;
	import src.Map.Camera;
	import src.Objects.SimpleGameObject;

	public class CityRegionObject extends Sprite implements IScrollableObject
	{
		public var type: int;
		public var groupId: int;
		public var objectId: int;
		
		private var objX: int;
		private var objY: int;
		
		public var extraProps: Object = new Object();
		
		public function CityRegionObject(type: int, groupId: int, objectId: int) {
			this.type = type;
			this.groupId = groupId;
			this.objectId = objectId;			
		}
		
		public function moveWithCamera(camera: Camera):void
		{
			x = objX - camera.x;
			y = objY - camera.y;
		}
		
		public function getX(): int
		{
			return objX;
		}
		
		public function getY(): int
		{
			return objY;
		}		
		
		public function setX(x: int):void
		{
			objX = x;
		}
		
		public function setY(y: int):void
		{
			objY = y;
		}
		
		public function setXY(x: int, y: int):void
		{
			setX(x);
			setY(y);
		}
	}
}


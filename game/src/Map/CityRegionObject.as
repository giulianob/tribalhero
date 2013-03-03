package src.Map
{
	import flash.display.*;
	import flash.events.MouseEvent;
	import flash.filters.GlowFilter;
	import flash.geom.ColorTransform;
	import flash.geom.Point;
	import src.Global;
	import src.Objects.Factories.ObjectFactory;
	import src.UI.Tooltips.MinimapInfoTooltip;
	import src.Util.BinaryList.*;
	import src.Util.Util;
	import src.Constants;
	import src.Map.Map;
	import src.Map.Camera;
	import src.Objects.SimpleGameObject;

	public class CityRegionObject extends Sprite
	{
		public var type: int;
		public var groupId: int;
		public var objectId: int;
			
		public var sprite: DisplayObject;
		
		public var extraProps: Object = new Object();
		
		public function CityRegionObject(type: int, groupId: int, objectId: int) {
			this.type = type;
			this.groupId = groupId;
			this.objectId = objectId;
		}			
	}
}


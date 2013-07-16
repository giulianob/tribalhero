package src.Map
{
    import flash.display.*;

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


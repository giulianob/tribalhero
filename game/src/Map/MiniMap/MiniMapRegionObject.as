package src.Map.MiniMap
{
    import flash.display.*;

    public class MiniMapRegionObject extends Sprite
	{
		public var type: int;
		public var groupId: int;
		public var objectId: int;
			
		public var sprite: DisplayObject;
		
		public var extraProps: Object = {};
		
		public function MiniMapRegionObject(type: int, groupId: int, objectId: int) {
			this.type = type;
			this.groupId = groupId;
			this.objectId = objectId;
		}			
	}
}


package src.Map.MiniMap
{
    import src.Map.*;
    import src.Objects.ObjectContainer;
    import src.UI.Tooltips.*;

    import starling.display.*;

    public class MiniMapRegion
	{
		public var id: int;
		private var objects: Array = [];
		private var mapFilter: MiniMapDrawer;
        private var objContainer: ObjectContainer;

		public function MiniMapRegion(id: int, filter: MiniMapDrawer, objContainer: ObjectContainer)
		{
			this.id = id;
			this.mapFilter = filter;
            this.objContainer = objContainer;
		}

		public function setFilter(filter:MiniMapDrawer): void
		{
			this.mapFilter = filter;
			for each(var obj: MiniMapRegionObject in objects) {
                filter.apply(obj);
            }
		}

		public function disposeData():void
		{
			for each(var obj: MiniMapRegionObject in objects) {
                objContainer.removeObject(obj);
            }

			objects = [];
		}

		public function addRegionObject(type: int, groupId: int, objectId: int, size: int, position: ScreenPosition, extraProps: *) : MiniMapRegionObject {
			var regionObject: MiniMapRegionObject = new MiniMapRegionObject(type, groupId, objectId, size, position, extraProps);

            objContainer.addObject(regionObject);

            new MinimapInfoTooltip(regionObject);
			
			mapFilter.apply(regionObject);
			objects.push(regionObject);
			return regionObject;
		}

        public static function sortOnId(a:MiniMapRegion, b:MiniMapRegion):Number
		{
			var aId:Number = a.id;
			var bId:Number = b.id;

			if(aId > bId) {
				return 1;
			} else if(aId < bId) {
				return -1;
			} else  {
				return 0;
			}
		}

		public static function compare(a: MiniMapRegion, value: int):int
		{
			return a.id - value;
		}
	}
}


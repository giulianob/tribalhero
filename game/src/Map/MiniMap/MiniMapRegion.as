package src.Map.MiniMap
{
    import flash.display.*;
    import flash.events.*;

    import src.*;
    import src.Map.*;
    import src.Map.MiniMapFilters.MiniMapRegionFilter;
    import src.UI.Tooltips.*;

    public class MiniMapRegion extends Sprite
	{
		public var id: int;
		private var globalX: int;
		private var globalY: int;
		private var objects: Array = [];
		private var filter: MiniMapRegionFilter;

		public function MiniMapRegion(id: int, filter: MiniMapRegionFilter)
		{
			this.id = id;
			this.filter = filter;

			globalX = (id % Constants.miniMapRegionRatioW) * Constants.miniMapRegionW;
			globalY = int(id / Constants.miniMapRegionRatioW) * (Constants.miniMapRegionH / 2);
			
			if (Constants.debug >= 4)
			{
				/* adds an outline to this region */
				graphics.beginFill(0x000000, 0);
				graphics.lineStyle(1, 0xcccccc, 0.3);
				graphics.drawRect(0, 0, Constants.miniMapRegionW * Constants.miniMapRegionTileW, Constants.miniMapRegionH * Constants.miniMapRegionTileH);
				graphics.endFill();
			}
		}
		
		public function setFilter(filter:MiniMapRegionFilter): void
		{
			this.filter = filter;
			for each(var obj: * in objects)
				filter.apply(obj);
		}

		public function disposeData():void
		{
			for each(var obj: * in objects)			
				Global.gameContainer.miniMap.objContainer.removeObject(obj);

			objects = [];
		}
		
		public function addRegionObject(type: int, groupId: int, objectId: int, size: int, position: ScreenPosition, extraProps: *) : MiniMapRegionObject {
			var regionObject: MiniMapRegionObject = new MiniMapRegionObject(type, groupId, objectId, size, position, extraProps);

			Global.gameContainer.miniMap.objContainer.addObject(regionObject);

			regionObject.addEventListener(MouseEvent.MOUSE_OVER, onObjectMouseOver);
			filter.apply(regionObject);
			objects.push(regionObject);
			return regionObject;
		}

		public function onObjectMouseOver(e: MouseEvent) : void {
			new MinimapInfoTooltip(e.target is MiniMapRegionObject ? e.target as MiniMapRegionObject : e.target.parent);
		}
		
		public function moveWithCamera(camera: Camera):void
		{
			x = globalX - camera.miniMapX - int(Constants.miniMapTileW / 2);
			y = globalY - camera.miniMapY - int(Constants.miniMapTileH / 2);
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


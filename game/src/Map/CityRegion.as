package src.Map
{
	import flash.display.*;
	import flash.events.*;
	import flash.geom.*;
	import flash.text.TextField;
	import src.*;
	import src.Map.*;
	import src.Map.CityRegionFilters.CityRegionFilter;
	import src.Objects.Factories.*;
	import src.UI.Tooltips.*;
	import src.Util.BinaryList.*;

	public class CityRegion extends Sprite
	{
		public static const DOT_SPRITE: String = "DOT_SPRITE_BLACK";

		public var id: int;
		private var globalX: int;
		private var globalY: int;
		private var objects: Array = new Array();
		private var filter: CityRegionFilter;

		public function CityRegion(id: int, filter: CityRegionFilter)
		{
			this.id = id;
			this.filter = filter;

			globalX = (id % Constants.miniMapRegionW) * Constants.cityRegionW;
			globalY = int(id / Constants.miniMapRegionW) * (Constants.cityRegionH / 2);
			
			if (Constants.debug >= 4)
			{
				/* adds an outline to this region */
				graphics.beginFill(0x000000, 0);
				graphics.lineStyle(1, 0xcccccc, 0.3);
				graphics.drawRect(0, 0, Constants.cityRegionW * Constants.cityRegionTileW, Constants.cityRegionH * Constants.cityRegionTileH);
				graphics.endFill();
			}
		}
		
		public function setFilter(filter:CityRegionFilter): void
		{
			this.filter = filter;
			for each(var obj: * in objects)
				filter.apply(obj);
		}

		public function disposeData():void
		{
			for each(var obj: * in objects)			
				Global.gameContainer.miniMap.objContainer.removeObject(obj);

			objects = new Array();
		}
		
		public function addRegionObject(type: int, groupId: int, objectId: int, objX: int, objY: int, extraProps: *) : CityRegionObject {		
			var coord: Point = MapUtil.getMiniMapScreenCoord(objX, objY);

			var regionObject: CityRegionObject = new CityRegionObject(type, groupId, objectId);
			regionObject.x = objX;
			regionObject.y = objY;
			
			regionObject.extraProps = extraProps;

			Global.gameContainer.miniMap.objContainer.addObject(regionObject);
					
			regionObject.addEventListener(MouseEvent.MOUSE_OVER, onObjectMouseOver);
			filter.apply(regionObject);
			objects.push(regionObject);
			return regionObject;
		}
				
		public function onObjectMouseOver(e: MouseEvent) : void {
			new MinimapInfoTooltip(e.target is CityRegionObject ? e.target as CityRegionObject : e.target.parent);
		}
		
		public function moveWithCamera(camera: Camera):void
		{
			x = globalX - camera.miniMapX - int(Constants.miniMapTileW / 2);
			y = globalY - camera.miniMapY - int(Constants.miniMapTileH / 2);
		}		
		public static function sortOnId(a:CityRegion, b:CityRegion):Number
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

		public static function compare(a: CityRegion, value: int):int
		{
			return a.id - value;
		}
	}
}


package src.Map
{	
	import flash.display.*;
	import flash.geom.Point;
	import flash.geom.Rectangle;
	import flash.utils.getDefinitionByName;
	import flash.display.DisplayObject;
	import src.GameContainer;
	import src.Objects.Factories.ObjectFactory;
	import src.Util.BinaryList;
	import src.Util.Util;
	
	import src.Constants;
	import src.Map.Map;
	import src.Map.Camera;
	import src.Objects.GameObject;
	import src.Objects.SimpleGameObject;
	import src.Objects.SimpleObject;
	
	public class CityRegion extends Sprite
	{
		public static const DOT_SPRITE: String = "DOT_SPRITE";
		public static const DOT_SPRITE_OWN: String = "DOT_SPRITE_ORANGE";
		
		public var id: int;
		private var globalX: int;
		private var globalY: int;
		private var objects: BinaryList = new BinaryList(SimpleGameObject.sortOnCityIdAndObjId, SimpleGameObject.compareCityIdAndObjId);
		private var map: Map;
		
		public function CityRegion(id: int, map: Map)
		{
			mouseEnabled = false;
			
			this.id = id;			
			
			this.map = map;
			
			/*createRegion();
			var screenRect: Sprite = new Sprite();
			screenRect.graphics.lineStyle(1, 0x00FF00);
			screenRect.graphics.drawRect(0, 0, Constants.cityRegionW, Constants.cityRegionH/2);
			addChild(screenRect);*/
			
			globalX = (id % Constants.miniMapRegionW) * Constants.cityRegionW;
			globalY = int(id / Constants.miniMapRegionW) * (Constants.cityRegionH / 2);		
			
			//trace("Create CityRegion " + id + " at " + globalX + " , " + globalY);
		}
		
		public function disposeData():void
		{
			for each(var gameObj: SimpleGameObject in objects.each())
			{				
				map.gameContainer.miniMap.objContainer.removeObject(gameObj);
			}
			
			objects.clear();
		}
		
		public function createRegion():void
		{				
			for (var a:int = 0; a < Math.ceil(Constants.cityRegionW / Constants.cityRegionBitmapW); a++)
			{
				for (var b:int = 0; b < Math.ceil(Constants.cityRegionH / Constants.cityRegionBitmapH); b++)
				{					
					createRegionPart(a * Constants.cityRegionBitmapTileW, b * Constants.cityRegionBitmapTileH);	
				}				
			}			
		}
		
		public function createRegionPart(x: int, y:int):void
		{
			var bg: Sprite = new Sprite();
			
			for (var b:int = 1; b <= Constants.cityRegionBitmapTileH + 1; b++)
			{
				for (var a:int = 1; a <= Constants.cityRegionBitmapTileW + 1; a++)
				{
					if (b-1+y >= Constants.cityRegionTileH || a-1+x >= Constants.cityRegionTileW) 
					{
						if (Constants.debug>=3)
							trace("OOB:" + (a + x) + "," + (b + y));
						
						break;
					}
				
					if (Constants.debug>=3)
						trace((a-1+x) + "," + (b-1+y));															
					
					var xadd:int = 0;
					var yadd:int = 0;
					
					if (b % 2 == 1) //odd tile
					   xadd =- (Constants.miniMapTileW / 2);
					
					var xcoord:int = Math.floor((a - 1) * Constants.miniMapTileW + xadd);
					var ycoord:int = Math.floor((b - 2) * (Constants.miniMapTileH / 2));
					
					if (xcoord <= Constants.cityRegionBitmapW + int(Constants.miniMapTileW * 1.5) || ycoord <= Constants.cityRegionBitmapH + Constants.miniMapTileH)
					{
						var tile: TILE_MINI = new TILE_MINI();
						tile.x = xcoord + int(Constants.miniMapTileW / 2);
						tile.y = ycoord + int(Constants.miniMapTileH / 2);
						bg.addChild(tile);
					}
				}				
			}
			
			bg.x = (x / Constants.cityRegionBitmapTileW) * Constants.cityRegionBitmapW;
			bg.y = (y / Constants.cityRegionBitmapTileH) * Constants.cityRegionBitmapH / 2;;
			
			addChild(bg);					
		}		
		
		public function sortObjects():void
		{
			objects.sort();
		}				
		
		public function objectAt(x: int, y: int): SimpleGameObject
		{			
			for each(var gameObj: SimpleGameObject in objects.each())
			{
				if (gameObj.getX() == x && gameObj.getY() == y && gameObj.visible)
					return gameObj;
			}
			
			return null;
		}
		
		public function addObject(level: int, type: int, playerId: int, cityId: int, objectId: int, hpPercent: int, objX: int, objY : int, resort: Boolean = true) : SimpleGameObject
		{			
			//trace("CityRegion.addObject " + id + " object " + type + " at " + objX + " , " + objY);
			
			var existingObj: SimpleGameObject = objects.get([cityId, objectId]);
			
			if (existingObj != null) //don't add if obj already exists
			{
				trace("Obj id " + objectId + " already exists in city region " + id);
				return null;
			}
			
			var obj: SimpleGameObject;
			
			if (map.cities.get(cityId))
				obj = ObjectFactory.getSimpleGameObject(DOT_SPRITE_OWN);
			else
				obj = ObjectFactory.getSimpleGameObject(DOT_SPRITE);
			
			if (obj == null)
				return null;			
			
			var gameObj: SimpleGameObject = (obj as SimpleGameObject);
			gameObj.name = "City Region Obj " + cityId;
			gameObj.init(map, playerId, cityId, objectId, type);
			
			var coord: Point = MapUtil.getMiniMapScreenCoord(objX, objY);
						
			gameObj.setProperties(level, hpPercent, coord.x, coord.y);			
			
			map.gameContainer.miniMap.objContainer.addObject(gameObj);
			
			objects.add(gameObj, resort);						
					
			return gameObj;
		}
		
		public function removeObject(cityId: int, objectId: int, dispose: Boolean = true): SimpleGameObject
		{		
			var gameObj: SimpleGameObject = objects.remove([cityId, objectId]);
			
			if (gameObj == null)
				return null;		
			
			map.gameContainer.miniMap.objContainer.removeObject(gameObj, 0, dispose);
			
			return gameObj;
		}
		
		public function getObject(cityId: int, objectId: int): SimpleGameObject
		{
			return objects.get([cityId, objectId]);			
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
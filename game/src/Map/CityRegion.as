package src.Map
{
	import flash.display.*;
	import flash.events.MouseEvent;
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

	public class CityRegion extends Sprite
	{
		public static const DOT_SPRITE: String = "DOT_SPRITE_BLACK";
		public static const DOT_SPRITE_OWN: String = "DOT_SPRITE";		

		public static const DIFFICULTY_COLORS: Array = [
		{ r: 200, g: 200, b: 200 },
		{ r: 0, g: 156, b: 20 },
		{ r: 255, g: 223, b: 0},
		{ r: 255, g: 169, b: 0 },
		{ r: 210, g: 0, b: 0 }
		];

		public var id: int;
		private var globalX: int;
		private var globalY: int;
		private var objects: BinaryList = new BinaryList(SimpleGameObject.sortOnCityIdAndObjId, SimpleGameObject.compareCityIdAndObjId);
		private var map: Map;

		public function CityRegion(id: int, map: Map)
		{
			this.id = id;

			this.map = map;

			//createRegion();

			/*
			var screenRect: Sprite = new Sprite();
			screenRect.graphics.lineStyle(1, 0x00FF00);
			screenRect.graphics.drawRect(0, 0, Constants.cityRegionW, Constants.cityRegionH/2);
			addChild(screenRect);
			*/

			globalX = (id % Constants.miniMapRegionW) * Constants.cityRegionW;
			globalY = int(id / Constants.miniMapRegionW) * (Constants.cityRegionH / 2);

			//Util.log("Create CityRegion " + id + " at " + globalX + " , " + globalY);
		}

		public function disposeData():void
		{
			for each(var gameObj: SimpleGameObject in objects.each())
			{
				src.Global.gameContainer.miniMap.objContainer.removeObject(gameObj);
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
						Util.log("OOB:" + (a + x) + "," + (b + y));

						break;
					}

					if (Constants.debug>=3)
					Util.log((a-1+x) + "," + (b-1+y));

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
			var existingObj: SimpleGameObject = objects.get([cityId, objectId]);

			if (existingObj != null) //don't add if obj already exists
			{
				Util.log("Obj id " + objectId + " already exists in city region " + id);
				return null;
			}

			var obj: SimpleGameObject;

			// If it's our city, we just show a special flag
			if (map.cities.get(cityId)) {
				obj = ObjectFactory.getSimpleGameObject(DOT_SPRITE_OWN);
			}
			else {
				obj = ObjectFactory.getSimpleGameObject(DOT_SPRITE);
				
				// Apply the difficulty transformation to the tile
				var levelDelta: int = level - Global.gameContainer.selectedCity.MainBuilding.level;
				var difficultyIdx: int;
				if (levelDelta <= -3) {
					difficultyIdx = 0;
				} else if (levelDelta <= 0) {
					difficultyIdx = 1;
				} else if (levelDelta <= 2) {
					difficultyIdx = 2;
				} else if (levelDelta <= 3) {
					difficultyIdx = 3;
				} else {
					difficultyIdx = 4;
				}

				obj.transform.colorTransform = new ColorTransform(1, 1, 1, 1, DIFFICULTY_COLORS[difficultyIdx].r, DIFFICULTY_COLORS[difficultyIdx].g, DIFFICULTY_COLORS[difficultyIdx].b);
			}

			//add object to map and objects list
			var gameObj: SimpleGameObject = (obj as SimpleGameObject);
			gameObj.name = "City Region Obj " + cityId;
			gameObj.init(map, playerId, cityId, objectId, type);

			var coord: Point = MapUtil.getMiniMapScreenCoord(objX, objY);

			gameObj.setProperties(level, hpPercent, coord.x, coord.y);

			src.Global.gameContainer.miniMap.objContainer.addObject(gameObj);

			gameObj.addEventListener(MouseEvent.MOUSE_OVER, onObjectMouseOver);
			gameObj.addEventListener(MouseEvent.MOUSE_OUT, onObjectMouseOut);

			objects.add(gameObj, resort);

			return gameObj;
		}

		public function removeObject(cityId: int, objectId: int, dispose: Boolean = true): SimpleGameObject
		{
			var gameObj: SimpleGameObject = objects.remove([cityId, objectId]);

			if (gameObj == null)
			return null;

			Global.gameContainer.miniMap.objContainer.removeObject(gameObj, 0, dispose);

			return gameObj;
		}

		public function getObject(cityId: int, objectId: int): SimpleGameObject
		{
			return objects.get([cityId, objectId]);
		}

		public function onObjectMouseOver(e: MouseEvent) : void {
			new MinimapInfoTooltip(e.target.parent);
		}

		public function onObjectMouseOut(e: MouseEvent) : void {

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


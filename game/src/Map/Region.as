package src.Map
{
	import flash.display.*;
	import flash.geom.Point;
	import flash.geom.Rectangle;
	import src.Objects.Factories.ObjectFactory;
	import src.Util.BinaryList.*;

	import src.Constants;
	import src.Map.Map;
	import src.Map.Camera;
	import src.Objects.GameObject;
	import src.Objects.SimpleGameObject;
	import src.Objects.SimpleObject;

	public class Region extends Sprite
	{
		public var id: int;
		private var tiles: Array;
		private var globalX: int;
		private var globalY: int;
		private var bitmapParts: Array;
		private var objects: BinaryList = new BinaryList(SimpleGameObject.sortOnCityIdAndObjId, SimpleGameObject.compareCityIdAndObjId);
		private var map: Map;

		public function Region(id: int, data: Array, map: Map)
		{
			mouseEnabled = false;

			this.id = id;
			tiles = data;
			bitmapParts = new Array();

			createRegion();

			this.map = map;

			globalX = (id % Constants.mapRegionW) * Constants.regionW;
			globalY = int(id / Constants.mapRegionW) * (Constants.regionH / 2);

			if (Constants.debug == 3)
			{
				/* adds an outline to this region */
				graphics.beginFill(0x000000, 0);
				graphics.lineStyle(2, 0x000000);
				graphics.drawRect(0, 0, width, height);
				graphics.endFill();
			}
		}

		// Removes all of the tiles from this region
		private function cleanTiles(): void {

			for (var i: int = 0; i < bitmapParts.length; i++)
			{
				removeChild(bitmapParts[i]);
				bitmapParts[i].bitmapData.dispose();
				bitmapParts[i] = null;
			}

			bitmapParts = new Array();
		}

		public function disposeData():void
		{
			cleanTiles();

			for each(var gameObj: SimpleGameObject in objects.each())
			{
				map.objContainer.removeObject(gameObj);
			}

			objects.clear();

			for (var i: int = numChildren - 1; i >= 0; i--) {
				removeChildAt(i);
			}

			objects = null;
			map = null;
			tiles = null;
		}

		public function createRegion():void
		{
			if (Constants.debug >= 2)
			trace("Creating region id: " + id + " " + globalX + "," + globalY);

			var tileset:TileSet = new TileSet(Constants.tileSetW, Constants.tileSetH);

			for (var a:int = 0; a < Math.ceil(Constants.regionW / Constants.regionBitmapW); a++)
			{
				for (var b:int = 0; b < Math.ceil(Constants.regionH / Constants.regionBitmapH); b++)
				{
					if (Constants.debug>=3)
					trace("Creating region part: " + (a * Constants.regionBitmapTileW) + "," + (b * Constants.regionBitmapTileH));

					createRegionPart(tileset, a * Constants.regionBitmapTileW, b * Constants.regionBitmapTileH);
					break;
				}
				break;
			}

			tileset.dispose();
			tileset = null;
		}

		public function sortObjects():void
		{
			objects.sort();
		}

		public function setTile(x: int, y: int, tileType: int, redraw: Boolean = true): void {
			var pt: Point = getTilePos(x, y);
			tiles[pt.y][pt.x] = tileType;
			if (redraw) { this.redraw(); }
		}

		public function redraw() : void {
			cleanTiles();
			createRegion();
		}

		public function createRegionPart(tileset:TileSet, x: int, y:int):void
		{
			var bg:Bitmap = new Bitmap(new BitmapData(Constants.regionBitmapW + Constants.tileW / 2, Constants.regionBitmapH / 2 + Constants.tileH / 2, true, 0));
			
			for (var bY:int = 1; bY <= Constants.regionBitmapTileH; bY++)
			{
				for (var aX:int = 1; aX <= Constants.regionBitmapTileW; aX++)
				{
					var tileid:int = tiles[bY-1+y][aX-1+x];

					var tilesetsrcX:int = int(tileid % Constants.tileSetTileW) * Constants.tileW;
					var tilesetsrcY:int = int(tileid / Constants.tileSetTileW) * Constants.tileH * 2;

					var xadd:int = 0;
					var yadd:int = 0;

					if ((bY % 2) == 1) //odd tile
					xadd = int(Constants.tileW / 2) * -1;

					var xcoord:int = int((aX - 1) * Constants.tileW + xadd);
					var ycoord:int = int((bY - 2) * (Constants.tileH / 2));

					var drawTo:Point = new Point(xcoord + int(Constants.tileW / 2), (ycoord + int(Constants.tileH / 2)) - (Constants.tileH));
					var srcRect:Rectangle = new Rectangle(tilesetsrcX, tilesetsrcY, Constants.tileW, Constants.tileH * 2);

					//trace(aX + "," + bY + " Tile Id:" + tileid + " draw to " + drawTo.x + "," + drawTo.y + " src " + srcRect.x + "," + srcRect.y + " " + srcRect.width + "," + srcRect.height);
					bg.bitmapData.copyPixels(tileset, srcRect, drawTo, null, null, true);
				}
			}

			bitmapParts.push(bg);

			bg.x = (x / Constants.regionBitmapTileW) * Constants.regionBitmapW;
			bg.y = (y / Constants.regionBitmapTileH) * (Constants.regionBitmapH / 2);

			addChild(bg);

			if (Constants.debug == 3)
			{
				/* adds an outline to each region part	*/
				graphics.beginFill(0, 0);
				graphics.lineStyle(2, 0x0000FF);
				graphics.drawRect(bg.x, bg.y, bg.bitmapData.width, bg.bitmapData.height);
				graphics.endFill();
			}
		}

		public function getObjectsAt(x: int, y: int, objClass: Class = null): Array
		{
			var objs: Array = new Array();
			for each(var gameObj: SimpleGameObject in objects.each())
			{
				if (objClass != null && !(gameObj is objClass)) continue;

				if (gameObj.getX() == x && gameObj.getY() == y && gameObj.visible) {
					objs.push(gameObj);
				}
			}

			return objs;
		}

		public function getTileAt(x: int, y: int) : int {
			var pt: Point = getTilePos(x, y);

			return tiles[pt.y][pt.x];
		}

		private function getTilePos(x: int, y: int) : Point {
			var regionStartingX: int = (id % Constants.mapRegionW);
			var regionStartingY: int = int(id / Constants.mapRegionW);

			x -= (regionStartingX * Constants.regionTileW);
			y -= (regionStartingY * Constants.regionTileH);

			return new Point(x, y);
		}

		public function addObject(level: int, type: int, playerId: int, cityId: int, objectId: int, hpPercent: int, objX: int, objY : int, resort: Boolean = true) : SimpleGameObject
		{
			var existingObj: SimpleObject = objects.get([cityId, objectId]);

			if (existingObj != null) //don't add if obj already exists
			{
				trace("Obj id " + objectId + " already exists in region " + id);
				return null;
			}

			var obj: SimpleObject = ObjectFactory.getInstance(type, level);

			if (obj == null)
			{
				return null;
			}

			var gameObj: SimpleGameObject = (obj as SimpleGameObject);
			gameObj.name = "Game Obj " + objectId;
			gameObj.init(map, playerId, cityId, objectId, type);

			var coord: Point = MapUtil.getScreenCoord(objX, objY);

			gameObj.setProperties(level, hpPercent, coord.x, coord.y);

			//set object callback when it's selected. Only gameobjects can be selected.
			if (gameObj is GameObject)
			(gameObj as GameObject).setOnSelect(map.selectObject);

			//add to object container and to internal list
			map.objContainer.addObject(gameObj);
			objects.add(gameObj, resort);

			//select object if the map is waiting for it to be selected
			if (map.selectViewable != null && map.selectViewable.cityId == gameObj.cityId && map.selectViewable.objectId == gameObj.objectId)
			{
				map.selectObject(gameObj as GameObject);
			}

			return gameObj;
		}

		public function addGameObject(gameObj: SimpleGameObject, resort: Boolean = true):void
		{
			removeObject(gameObj.cityId, gameObj.objectId, false);

			map.objContainer.addObject(gameObj);

			objects.add(gameObj, resort);
		}

		public function removeObject(cityId: int, objectId: int, dispose: Boolean = true): SimpleGameObject
		{
			var gameObj: SimpleGameObject = objects.remove([cityId, objectId]);

			if (gameObj == null)
			return null;

			map.objContainer.removeObject(gameObj, 0, dispose);

			return gameObj;
		}

		public function getObject(cityId: int, objectId: int): SimpleGameObject
		{
			return objects.get([cityId, objectId]);
		}

		public function moveWithCamera(camera: Camera):void
		{
			x = globalX - camera.x - int(Constants.tileW / 2);
			y = globalY - camera.y - int(Constants.tileH / 2);
		}

		public static function sortOnId(a:Region, b:Region):Number
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

		public static function compare(a: Region, value: int):int
		{
			return a.id - value;
		}
	}
}


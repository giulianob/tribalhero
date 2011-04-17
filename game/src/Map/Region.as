package src.Map
{
	import flash.display.*;
	import flash.geom.Point;
	import flash.geom.Rectangle;
	import src.Constants;
	import src.Global;
	import src.Map.Camera;
	import src.Map.Map;
	import src.Objects.Factories.ObjectFactory;
	import src.Objects.GameObject;
	import src.Objects.NewCityPlaceholder;
	import src.Objects.SimpleGameObject;
	import src.Objects.SimpleObject;
	import src.Util.BinaryList.*;
	import src.Util.Util;

	public class Region extends Sprite
	{
		public var id: int;
		private var tiles: Array;
		private var globalX: int;
		private var globalY: int;
		private var bitmapParts: Array;
		private var objects: BinaryList = new BinaryList(SimpleGameObject.sortOnGroupIdAndObjId, SimpleGameObject.compareGroupIdAndObjId);
		private var placeHolders: BinaryList = new BinaryList(SimpleObject.sortOnXandY, SimpleObject.compareXAndY);
		private var map: Map;
		
		public function Region(id: int, data: Array, map: Map)
		{
			mouseEnabled = false;

			this.id = id;
			this.map = map;			
			this.tiles = data;
			
			bitmapParts = new Array();			

			globalX = (id % Constants.mapRegionW) * Constants.regionW;
			globalY = int(id / Constants.mapRegionW) * (Constants.regionH / 2);
			
			createRegion();

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
			clearAllPlaceholders();
			cleanTiles();

			for each(var gameObj: SimpleGameObject in objects.each())		
				map.objContainer.removeObject(gameObj);			

			objects.clear();

			for (var i: int = numChildren - 1; i >= 0; i--)
				removeChildAt(i);			
				
			objects = null;
			map = null;
			tiles = null;
		}

		public function createRegion():void
		{			
			if (Constants.debug >= 2)
				Util.log("Creating region id: " + id + " " + globalX + "," + globalY);
			
			for (var a:int = 0; a < Math.ceil(Constants.regionW / Constants.regionBitmapW); a++)
			{
				for (var b:int = 0; b < Math.ceil(Constants.regionH / Constants.regionBitmapH); b++)
				{
					if (Constants.debug>=3)
						Util.log("Creating region part: " + (a * Constants.regionBitmapTileW) + "," + (b * Constants.regionBitmapTileH));

					createRegionPart(Constants.tileset, a * Constants.regionBitmapTileW, b * Constants.regionBitmapTileH);
					break;
				}
				break;
			}
		}

		public function sortObjects():void
		{
			objects.sort();
		}

		public function setTile(x: int, y: int, tileType: int, redraw: Boolean = true): void {
			var pt: Point = getTilePos(x, y);
			
			tiles[pt.y][pt.x] = tileType;
			
			clearPlaceholders(x, y);
			addPlaceholderObjects(tileType, x, y);
			
			if (redraw) 
				this.redraw();
		}

		public function redraw() : void {
			cleanTiles();
			createRegion();
		}

		public function createRegionPart(tileset:TileSet, x: int, y:int):void
		{
			var bg:Bitmap = new Bitmap(new BitmapData(Constants.regionBitmapW + Constants.tileW / 2, Constants.regionBitmapH / 2 + Constants.tileH / 2, true, 0));
			bg.smoothing = false;
			
			var tileHDiv2: int = Constants.tileH / 2;
			var tileHTimes2: int = Constants.tileH * 2;
			var tileWDiv2: int = Constants.tileW / 2;
			var tileWTimes2: int = Constants.tileW * 2;
			var oddShift: int = int(Constants.tileW / 2) * -1;
			var regionStartingX: int = (id % Constants.mapRegionW) * Constants.regionTileW;
			var regionStartingY: int = int(id / Constants.mapRegionW) * Constants.regionTileH;
			
			for (var bY:int = 1; bY <= Constants.regionBitmapTileH; bY++)
			{
				for (var aX:int = 1; aX <= Constants.regionBitmapTileW; aX++)
				{
					var tileX: int = aX - 1 + x;
					var tileY: int = bY - 1 + y;
					var tileid:int = tiles[tileY][tileX];
					
					addPlaceholderObjects(tileid, tileX + regionStartingX, tileY + regionStartingY);

					var tilesetsrcX:int = int(tileid % Constants.tileSetTileW) * Constants.tileW;
					var tilesetsrcY:int = int(tileid / Constants.tileSetTileW) * tileHTimes2;

					var xadd:int = 0;
					var yadd:int = 0;

					if ((bY % 2) == 1) //odd tile
						xadd = oddShift;

					var xcoord:int = int((aX - 1) * Constants.tileW + xadd);
					var ycoord:int = int((bY - 2) * tileHDiv2);

					var drawTo:Point = new Point(xcoord + tileWDiv2, (ycoord + tileHDiv2) - (Constants.tileH));
					var srcRect:Rectangle = new Rectangle(tilesetsrcX, tilesetsrcY, Constants.tileW, tileHTimes2);

					//Util.log(aX + "," + bY + " Tile Id:" + tileid + " draw to " + drawTo.x + "," + drawTo.y + " src " + srcRect.x + "," + srcRect.y + " " + srcRect.width + "," + srcRect.height);
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
		
		private function clearPlaceholders(x: int, y: int) : void
		{
			var coord: Point = MapUtil.getScreenCoord(x, y);
			var objs: Array = placeHolders.getRange([x, y]);
			
			for each (var obj: SimpleObject in objs)
				map.objContainer.removeObject(obj);
			
			placeHolders.clear();
		}
		
		private function clearAllPlaceholders() : void
		{
			for each (var obj: SimpleObject in placeHolders.each())
				map.objContainer.removeObject(obj);
				
			placeHolders.clear();
		}		
		
		private function addPlaceholderObjects(tileId: int, x: int, y: int) : void 
		{	
			return;
			if (tileId == Constants.cityStartTile) {
				var coord: Point = MapUtil.getScreenCoord(x, y);
				var obj: NewCityPlaceholder = ObjectFactory.getNewCityPlaceholderInstance();
				obj.setX(coord.x);
				obj.setY(coord.y);
				obj.setOnSelect(Global.map.selectObject);
				map.objContainer.addObject(obj);				
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

		public function addObject(gameObj: SimpleGameObject, resort: Boolean = true) : SimpleGameObject
		{
			if (resort) {
				var existingObj: SimpleObject = objects.get([gameObj.groupId, gameObj.objectId]);

				if (existingObj != null) //don't add if obj already exists
				{
					Util.log("Obj " + gameObj.groupId + ", " + gameObj.objectId + " already exists in region " + id);
					return null;
				}
			}

			//add to object container and to internal list
			map.objContainer.addObject(gameObj);
			objects.add(gameObj, resort);
			
			//select object if the map is waiting for it to be selected
			if (map.selectViewable != null && map.selectViewable.groupId == gameObj.groupId && map.selectViewable.objectId == gameObj.objectId)			
				map.selectObject(gameObj as GameObject);			
				
			if (resort)
				sortObjects();

			return gameObj;
		}

		public function removeObject(groupId: int, objectId: int, dispose: Boolean = true): SimpleGameObject
		{
			var gameObj: SimpleGameObject = objects.remove([groupId, objectId]);

			if (gameObj == null)
				return null;	

			map.objContainer.removeObject(gameObj, 0, dispose);

			return gameObj;
		}

		public function getObject(groupId: int, objectId: int): SimpleGameObject
		{
			return objects.get([groupId, objectId]);
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


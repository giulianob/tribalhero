package src.Map
{
    import flash.display.*;
    import flash.geom.*;
    import flash.text.*;

    import src.*;
    import src.Objects.*;
    import src.Objects.Factories.*;
    import src.Util.*;
    import src.Util.BinaryList.*;

    public class Region extends Sprite
    {
        public var id: int;
        private var tiles: Array;
        private var globalX: int;
        private var globalY: int;
        private var bg: Bitmap;
        private var primaryObjects: RegionObjectList = new RegionObjectList();
        private var tileObjects: RegionObjectList = new RegionObjectList();
        private var placeHolders: BinaryList = new BinaryList(SimpleObject.sortOnXandY, SimpleObject.compareXAndY);
        private var map: Map;

        private var regionPosition: Position;

        public function Region(id: int, data: Array, map: Map)
        {
            mouseEnabled = false;
            mouseChildren = false;

            this.id = id;
            this.map = map;
            this.tiles = data;

            regionPosition = new Position(
                    (id % Constants.mapRegionW) * Constants.regionTileW,
                    int(id / Constants.mapRegionW) * Constants.regionTileH);

            globalX = (id % Constants.mapRegionW) * Constants.regionW;
            globalY = int(id / Constants.mapRegionW) * (Constants.regionH / 2);

            createRegion();

			if (Constants.debug >= 4)
            {
                /* adds an outline to this region */
                graphics.beginFill(0x000000, 0);
                graphics.lineStyle(3, 0x000000);
                graphics.drawRect(0, 0, width, height);
                graphics.endFill();
            }
        }

        public function disposeData():void
        {
            clearAllPlaceholders();

            bg.bitmapData.dispose();
            bg = null;

            for each(var gameObj: SimpleGameObject in primaryObjects.allObjects()) {
                map.objContainer.removeObject(gameObj);
            }

            primaryObjects.clear();
            tileObjects.clear();

            if (numChildren > 0) {
                removeChildren(0, numChildren - 1);
            }

            primaryObjects = null;
            map = null;
            tiles = null;
        }

        private function createRegion():void
        {
            if (Constants.debug >= 2)
                Util.log("Creating region id: " + id + " " + globalX + "," + globalY);

            bg = new Bitmap(new BitmapData(Constants.regionW + Constants.tileW / 2, Constants.regionH / 2 + Constants.tileH / 2, true, 0));
            bg.smoothing = false;

            drawRegion();

            bg.x = (x / Constants.regionTileW) * Constants.regionW;
            bg.y = (y / Constants.regionTileH) * (Constants.regionH / 2);

            addChild(bg);
        }

        private function drawRegion(): void {
            bg.bitmapData.fillRect(new Rectangle(0, 0, bg.bitmapData.width, bg.bitmapData.height), 0xadb957);

            var tileHDiv2: int = Constants.tileH / 2;
            var tileWDiv2: int = Constants.tileW / 2;

            clearAllPlaceholders();

            // tileX and tileY represent the tile relative to the region
            for (var tileY:int = 0; tileY < Constants.regionTileH; tileY++)
            {
                var oddShift: int = (tileY % 2) == 0 ? Constants.tileW / -2 : 0;

                for (var tileX:int = 0; tileX < Constants.regionTileW; tileX++)
                {
                    var tileid:int = tiles[tileY][tileX];

                    // The position of this tile for the entire world
                    var mapTilePosition: Position = new Position(tileX + regionPosition.x, tileY + regionPosition.y);

                    addPlaceholderObjects(tileid, mapTilePosition);

                    var tilesetsrcX:int = int(tileid % Constants.tileSetTileW) * Constants.tileW;
                    var tilesetsrcY:int = int(tileid / Constants.tileSetTileW) * Constants.tileH;

                    var drawTo:Point = new Point(
                            tileX * Constants.tileW + oddShift + tileWDiv2,
                            tileY * tileHDiv2);

                    bg.bitmapData.copyPixels(
                            Constants.tileset.bitmapData,
                            new Rectangle(tilesetsrcX, tilesetsrcY, Constants.tileW, Constants.tileH),
                            drawTo,
                            null,
                            null,
                            true);

                    if (Constants.debug >= 4)
                    {
                        var txtCoords: TextField = new TextField();
                        txtCoords.text = mapTilePosition.x + "," + mapTilePosition.y;
                        var coordsBitmap:BitmapData = new BitmapData(txtCoords.width, txtCoords.height, true, 0);
                        coordsBitmap.draw(txtCoords);
                        bg.bitmapData.copyPixels(coordsBitmap,
                                new Rectangle(0, 0, txtCoords.width, txtCoords.height),
                                new Point(drawTo.x + txtCoords.width/2, drawTo.y + txtCoords.textHeight),
                                null,
                                null,
                                true);
                        coordsBitmap.dispose();
                    }
                }
            }
        }

        public function setTile(position: Position, tileType: int): void {
            var relativeTilePosX: int = position.x - regionPosition.x;
            var relativeTilePosY: int = position.y - regionPosition.y;

            tiles[relativeTilePosY][relativeTilePosX] = tileType;
        }

        public function redraw() : void {
            drawRegion();
        }

        private function clearPlaceholders(position: ScreenPosition) : void
        {
            var objs: Array = placeHolders.getRange([position.x, position.y]);

            for each (var obj: SimpleObject in objs) {
                map.objContainer.removeObject(obj);
            }

            placeHolders.removeRange([position.x, position.y]);
        }

        private function clearAllPlaceholders() : void
        {
            for each (var obj: SimpleObject in placeHolders) {
                map.objContainer.removeObject(obj);
            }

            placeHolders.clear();
        }

        private function addPlaceholderObjects(tileId: int, position: Position) : void
        {
            return;

            if (tileId == Constants.cityStartTile) {
                var screenPosition: ScreenPosition = position.toScreenPosition();

                if (getObjectsInTile(position).length > 0) {
                    return;
                }

                var obj: NewCityPlaceholder = ObjectFactory.getNewCityPlaceholderInstance(screenPosition.x, screenPosition.y);
                obj.setOnSelect(Global.map.selectObject);
                map.objContainer.addObject(obj);
                placeHolders.add(obj);
            }
        }

        public function getObjectsInTile(position: Position, objClass: * = null): Array
        {
            var objs: Array = [];

            for each(var gameObj: SimpleGameObject in tileObjects.get(position))
            {
                if (objClass != null) {
                    if (objClass is Array) {
                        var typeOk: Boolean = false;
                        for each (var type: Class in objClass) {
                            if (gameObj is type) {
                                typeOk = true;
                                break;
                            }
                        }
                        if (!typeOk) {
                            continue;
                        }
                    }
                    else if (!(gameObj is objClass)) {
                        continue;
                    }
                }

                if (gameObj.visible) {
                    objs.push(gameObj);
                }
            }

            return objs;
        }

        public function getTileAt(position: Position) : int {
            var x: int = position.x - regionPosition.x;
            var y: int = position.y - regionPosition.y;

            return tiles[y][x];
        }

        public function addObjectToTile(gameObj: SimpleGameObject, pos: Position): void {
            tileObjects.add(gameObj, pos);
        }

        public function removeObjectFromTile(gameObj: SimpleGameObject, pos: Position): SimpleGameObject {
            return tileObjects.remove(gameObj, pos);
        }

        public function addObject(gameObj: SimpleGameObject) : Boolean
        {
            if (primaryObjects.getById(gameObj.groupId, gameObj.objectId)) {
                return false;
            }

            var objMapPos: Position = gameObj.primaryPosition.toPosition();
            clearPlaceholders(gameObj.primaryPosition);

            //add to object container and to internal list
            primaryObjects.add(gameObj, objMapPos);

            return true;
        }

        public function removeObject(obj: SimpleGameObject): SimpleGameObject
        {
            return primaryObjects.remove(obj, obj.primaryPosition.toPosition());
        }

        public function getObject(groupId: int, objectId: int): SimpleGameObject
        {
            return primaryObjects.getById(groupId, objectId);
        }

        public function moveWithCamera(camera: Camera):void
        {
            x = globalX - camera.currentPosition.x - int(Constants.tileW / 2);
            y = globalY - camera.currentPosition.y - int(Constants.tileH / 2);
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


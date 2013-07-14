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
        private var bitmapParts: Array;
        private var objects: BinaryList = new BinaryList(SimpleGameObject.sortOnGroupIdAndObjId, SimpleGameObject.compareGroupIdAndObjId);
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

            bitmapParts = [];

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

        // Removes all of the tiles from this region
        private function cleanTiles(): void {

            for (var i: int = 0; i < bitmapParts.length; i++)
            {
                removeChild(bitmapParts[i]);
                bitmapParts[i].bitmapData.dispose();
                bitmapParts[i] = null;
            }

            bitmapParts = [];
        }

        public function disposeData():void
        {
            clearAllPlaceholders();
            cleanTiles();

            for each(var gameObj: SimpleGameObject in objects)
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

            clearAllPlaceholders();


            var bg:Bitmap = new Bitmap(new BitmapData(Constants.regionW + Constants.tileW / 2, Constants.regionH / 2 + Constants.tileH / 2, true, 0));
            bg.smoothing = false;

            var tileHDiv2: int = Constants.tileH / 2;
            var tileHTimes2: int = Constants.tileH * 2;
            var tileWDiv2: int = Constants.tileW / 2;

            // tileX and tileY represent the tile relative to the region
            for (var tileY:int = 0; tileY < Constants.regionTileH; tileY++)
            {
                var oddShift: int = (tileY % 2) == 0 ? Constants.tileW / -2 : 0;

                for (var tileX:int = 0; tileX < Constants.regionTileW; tileX++)
                {
                    var tileid:int = tiles[tileY][tileX];

                    // The position of this tile for the entire world
                    var mapTilePosition: Position = new Position(tileX + regionPosition.x, tileY + regionPosition.y);

                    addPlaceholderObjects(tileid, mapTilePosition.toScreenPosition());

                    var tilesetsrcX:int = int(tileid % Constants.tileSetTileW) * Constants.tileW;
                    var tilesetsrcY:int = int(tileid / Constants.tileSetTileW) * tileHTimes2;

                    var drawTo:Point = new Point(
                            tileX * Constants.tileW + oddShift + tileWDiv2,
                            /* We subtract tileH because the tile graphic in the tileset is twice as high */
                            tileY * tileHDiv2 - Constants.tileH);

                    bg.bitmapData.copyPixels(
                            Constants.tileset.bitmapData,
                            new Rectangle(tilesetsrcX, tilesetsrcY, Constants.tileW, tileHTimes2),
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
                                // The y draw position needs to account for the fact that the tile is actually twice as high as tileH in the drawTo
                                new Point(drawTo.x + txtCoords.width/2, drawTo.y + Constants.tileH + txtCoords.textHeight),
                                null,
                                null,
                                true);
                        coordsBitmap.dispose();
                    }
                }
            }

            bitmapParts.push(bg);

            bg.x = (x / Constants.regionTileW) * Constants.regionW;
            bg.y = (y / Constants.regionTileH) * (Constants.regionH / 2);

            addChild(bg);
        }

        public function sortObjects():void
        {
            objects.sort();
        }

        public function setTile(x: int, y: int, tileType: int, redraw: Boolean = true): void {
            var pt: Position = getTilePos(x, y);

            tiles[pt.y][pt.x] = tileType;

            clearPlaceholders(x, y);
            addPlaceholderObjects(tileType, new ScreenPosition(x, y));

            if (redraw)
                this.redraw();
        }

        public function redraw() : void {
            cleanTiles();
            createRegion();
        }

        private function clearPlaceholders(x: int, y: int) : void
        {
            var coord: Point = TileLocator.getScreenCoord(x, y);
            var objs: Array = placeHolders.getRange([coord.x, coord.y]);

            for each (var obj: SimpleObject in objs) {
                map.objContainer.removeObject(obj);
            }

            placeHolders.removeRange([coord.x, coord.y]);
        }

        private function clearAllPlaceholders() : void
        {
            for each (var obj: SimpleObject in placeHolders)
                map.objContainer.removeObject(obj);

            placeHolders.clear();
        }

        private function addPlaceholderObjects(tileId: int, screenPosition: ScreenPosition) : void
        {
            if (tileId == Constants.cityStartTile) {
                if (getObjectsAt(screenPosition).length > 0) {
                    return;
                }

                var obj: NewCityPlaceholder = ObjectFactory.getNewCityPlaceholderInstance(screenPosition.x, screenPosition.y);
                obj.setOnSelect(Global.map.selectObject);
                map.objContainer.addObject(obj);
                placeHolders.add(obj);
            }
        }

        public function getObjectsAt(screenPosition: ScreenPosition, objClass: * = null): Array
        {
            var objs: Array = [];
            for each(var gameObj: SimpleGameObject in objects)
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

                if (gameObj.objX == screenPosition.x && gameObj.objY == screenPosition.y && gameObj.visible) {
                    objs.push(gameObj);
                }
            }

            return objs;
        }

        public function getTileAt(x: int, y: int) : int {
            var pt: Position = getTilePos(x, y);

            return tiles[pt.y][pt.x];
        }

        private function getTilePos(x: int, y: int) : Position {
            x -= regionPosition.x;
            y -= regionPosition.y;

            return new Position(x, y);
        }

        public function addObject(gameObj: SimpleGameObject, sorted: Boolean = true) : SimpleGameObject
        {
            if (sorted) {
                var existingObj: SimpleObject = objects.get([gameObj.groupId, gameObj.objectId]);

                if (existingObj != null)
                {
                    return null;
                }
            }

            var objMapPos: Point = TileLocator.getMapCoord(gameObj.objX, gameObj.objY);
            clearPlaceholders(objMapPos.x, objMapPos.y);

            //add to object container and to internal list
            map.objContainer.addObject(gameObj);
            objects.add(gameObj, sorted);

            //select object if the map is waiting for it to be selected
            if (map.selectViewable != null && map.selectViewable.groupId == gameObj.groupId && map.selectViewable.objectId == gameObj.objectId)
                map.selectObject(gameObj as GameObject);

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


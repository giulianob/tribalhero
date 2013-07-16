package src.Map {
    import flash.utils.Dictionary;

    import src.Objects.SimpleGameObject;

    public class RegionObjectList {
        private var objects: Dictionary = new Dictionary();

        public var count: int;

        public function clear(): void {
            objects = new Dictionary();
        }

        public function add(obj: SimpleGameObject, position: Position): void
        {
            var index: int = TileLocator.getTileIndex(position);

            if (objects[index] == null) {
                objects[index] = [obj];
            }
            else {
                objects[index].push(obj);
            }

            count++;
        }

        public function remove(obj: SimpleGameObject, position: Position): SimpleGameObject {
            var index: int = TileLocator.getTileIndex(position);

            var tileObjects: Array = objects[index];
            if (tileObjects == null) {
                return null;
            }

            for (var i: int = 0; i < tileObjects.length; i++)
            {
                var tileObject: SimpleGameObject = tileObjects[i];

                if (obj.groupId != tileObject.groupId || obj.objectId != tileObject.objectId) {
                    continue;
                }

                tileObjects.splice(i, 1);
                return tileObject;
            }

            return null;
        }

        public function removeById(groupId: int, objectId: int): SimpleGameObject
        {
            for each (var tileList:Array in objects) {
                for (var itemIndex: int = 0; itemIndex < tileList.length; itemIndex++) {
                    var obj: SimpleGameObject = tileList[itemIndex];

                    if (obj.groupId != groupId || obj.objectId != objectId) {
                        continue;
                    }

                    tileList.splice(itemIndex, 1);
                    count--;

                    return obj;
                }
            }

            return null;
        }

        public function getById(groupId: int, objectId: int): SimpleGameObject
        {
            for each (var tileList:Array in objects) {
                for each (var obj: SimpleGameObject in tileList) {
                    if (obj.groupId == groupId && obj.objectId == objectId) {
                        return obj;
                    }
                }
            }

            return null;
        }

        public function get(position: Position): Array
        {
            var index: int = TileLocator.getTileIndex(position);

            var tileList: Array = objects[index];

            if (tileList === null) {
                return [];
            }

            return tileList.concat();
        }

        public function allObjects(): Array
        {
            var allObjects: Array = [];

            for each (var value:Array in objects) {
                allObjects = allObjects.concat(value);
            }

            return allObjects;
        }
    }
}
